using System;
using System.Threading.Tasks;
using System.Windows.Forms;

using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace EQ2ModelViewer
{
    public class FontShaderClass
    {
        private struct MatrixBufferType
        {
            public Matrix world;
            public Matrix view;
            public Matrix projection;
        }

        private struct PixelBufferType
        {
            public Vector4 pixelColor;
        }

        private VertexShader m_VertexShader;
        private PixelShader m_PixelShader;
        private InputLayout m_Layout;
        private Buffer m_MatrixBuffer;
        private SamplerState m_SamplerState;
        private Buffer m_PixelBuffer;

        public bool Initialize(Device device)
        {
            return InitializeShader(device, "Font.vs", "Font.ps");
        }

        private bool InitializeShader(Device device, string vertexShader, string pixelShader)
        {
            ShaderSignature inputSignature;
            string error;
            // load and compile the vertex shader
            using (var bytecode = ShaderBytecode.CompileFromFile(vertexShader, "FontVertexShader", "vs_5_0", ShaderFlags.EnableStrictness, EffectFlags.None, null, null, out error))
            {
                inputSignature = ShaderSignature.GetInputSignature(bytecode);
                m_VertexShader = new VertexShader(device, bytecode);
            }

            if (m_VertexShader == null)
            {
                Console.WriteLine("InitializeShader: Error creating vertex shader: " + error);
                return false;
            }

            // load and compile the pixel shader
            using (var bytecode = ShaderBytecode.CompileFromFile(pixelShader, "FontPixelShader", "ps_5_0", ShaderFlags.EnableStrictness, EffectFlags.None, null, null, out error))
                m_PixelShader = new PixelShader(device, bytecode);

            if (m_PixelShader == null)
            {
                Console.WriteLine("InitializeShader: Error creating pixel shader: " + error);
                return false;
            }

            var elements = new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, -1, 0, InputClassification.PerVertexData, 0) };
            m_Layout = new InputLayout(device, inputSignature, elements);

            m_MatrixBuffer = new Buffer(device, System.Runtime.InteropServices.Marshal.SizeOf(typeof(MatrixBufferType)), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            if (m_MatrixBuffer == null)
            {
                Console.WriteLine("InitializeShader: Unable to create the matrix buffer.");
                return false;
            }

            SamplerDescription samplerDesc = new SamplerDescription();
            samplerDesc.Filter = Filter.MinMagMipLinear;
            samplerDesc.AddressU = TextureAddressMode.Wrap;
            samplerDesc.AddressV = TextureAddressMode.Wrap;
            samplerDesc.AddressW = TextureAddressMode.Wrap;
            samplerDesc.MipLodBias = 0.0f;
            samplerDesc.MaximumAnisotropy = 1;
            samplerDesc.ComparisonFunction = Comparison.Always;
            samplerDesc.BorderColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
            samplerDesc.MinimumLod = 0;
            samplerDesc.MaximumLod = 3.402823466e+38f; // from d3d11.h, #define D3D11_FLOAT32_MAX	( 3.402823466e+38f )

            m_SamplerState = SamplerState.FromDescription(device, samplerDesc);
            if (m_SamplerState == null)
            {
                Console.WriteLine("InitializeShader: Unable to create the sampler state.");
                return false;
            }

            m_PixelBuffer = new Buffer(device, System.Runtime.InteropServices.Marshal.SizeOf(typeof(PixelBufferType)), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            if (m_PixelBuffer == null)
            {
                Console.WriteLine("InitializeShader: Unable to create the pixel buffer.");
                return false;
            }

            return true;
        }

        public void ShutDown()
        {
            m_VertexShader.Dispose();
            m_PixelShader.Dispose();
            m_Layout.Dispose();
            m_MatrixBuffer.Dispose();
            m_SamplerState.Dispose();
            m_PixelBuffer.Dispose();
        }

        private bool SetShaderParameters(DeviceContext context, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture, Vector4 PixelColor)
        {
            Matrix.Transpose(ref worldMatrix, out worldMatrix);
            Matrix.Transpose(ref viewMatrix, out viewMatrix);
            Matrix.Transpose(ref projectionMatrix, out projectionMatrix);


            var mappedResource = context.MapSubresource(m_MatrixBuffer, 0, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);
            MatrixBufferType data = new MatrixBufferType();
            data.world = worldMatrix;
            data.view = viewMatrix;
            data.projection = projectionMatrix;
            mappedResource.Data.Write<MatrixBufferType>(data);
            context.UnmapSubresource(m_MatrixBuffer, 0);

            context.VertexShader.SetConstantBuffer(m_MatrixBuffer, 0);
            context.PixelShader.SetShaderResource(texture, 0);

            mappedResource = context.MapSubresource(m_PixelBuffer, 0, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);
            PixelBufferType data2 = new PixelBufferType();
            data2.pixelColor = PixelColor;
            mappedResource.Data.Write<PixelBufferType>(data2);
            context.UnmapSubresource(m_PixelBuffer, 0);

            context.PixelShader.SetConstantBuffer(m_PixelBuffer, 0);

            return true;
        }

        public bool Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture, Vector4 PixelColor)
        {
            if (!SetShaderParameters(context, worldMatrix, viewMatrix, projectionMatrix, texture, PixelColor))
                return false;

            RenderShader(context, indexCount);

            return true;
        }

        private void RenderShader(DeviceContext context, int indexCount)
        {
            // Set the vertex input layout
            context.InputAssembler.InputLayout = m_Layout;

            // Set the vertex and pixel shaders that will be used to render
            context.VertexShader.Set(m_VertexShader);
            context.PixelShader.Set(m_PixelShader);

            // Set the sampler state in the pixel shader
            context.PixelShader.SetSampler(m_SamplerState, 0);

            // Render
            context.DrawIndexed(indexCount, 0, 0);
        }
    }
}

