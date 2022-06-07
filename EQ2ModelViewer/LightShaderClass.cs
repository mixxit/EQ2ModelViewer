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
    public class LightShaderClass
    {
        private struct MatrixBufferType
        {
            public Matrix world;
            public Matrix view;
            public Matrix projection;
        }

        private struct CameraBufferType
        {
            public Vector3 cameraPosition;
            public float padding;
        }

        private struct LightBufferType
        {
            public Vector4 ambientColor;
            public Vector4 diffuseColor;
            public Vector3 lightDirection;
            public float specularPower;
            public Vector4 specularColor;
        }

        private VertexShader m_VertexShader;
        private PixelShader m_PixelShader;
        private InputLayout m_Layout;
        private Buffer m_MatrixBuffer;
        private SamplerState m_SamplerState;
        private Buffer m_CameraBuffer;
        private Buffer m_LightBuffer;

        public bool Initialize(Device device)
        {
            return InitializeShader(device, "Light.vs", "Light.ps");
        }

        private bool InitializeShader(Device device, string vertexShader, string pixelShader)
        {
            ShaderSignature inputSignature;
            string error;
            // load and compile the vertex shader
            using (var bytecode = ShaderBytecode.CompileFromFile(vertexShader, "LightVertexShader", "vs_5_0", ShaderFlags.EnableStrictness, EffectFlags.None, null, null, out error))
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
            using (var bytecode = ShaderBytecode.CompileFromFile(pixelShader, "LightPixelShader", "ps_5_0", ShaderFlags.EnableStrictness, EffectFlags.None, null, null, out error))
                m_PixelShader = new PixelShader(device, bytecode);

            if (m_PixelShader == null)
            {
                Console.WriteLine("InitializeShader: Error creating pixel shader: " + error);
                return false;
            }

            var elements = new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, -1, 0, InputClassification.PerVertexData, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, -1, 0, InputClassification.PerVertexData, 0) };
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

            m_CameraBuffer = new Buffer(device, System.Runtime.InteropServices.Marshal.SizeOf(typeof(CameraBufferType)), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            if (m_CameraBuffer == null)
            {
                Console.WriteLine("InitializeShader: Unable to create the camera buffer.");
                return false;
            }

            m_LightBuffer = new Buffer(device, System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightBufferType)), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            if (m_LightBuffer == null)
            {
                Console.WriteLine("InitializeShader: Unable to create the light buffer.");
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
            m_CameraBuffer.Dispose();
            m_LightBuffer.Dispose();
        }

        private bool SetShaderParameters(DeviceContext context, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture, Vector3 lightDirection, Vector4 ambientColor, Vector4 diffuseColor, Vector3 cameraPosition, Vector4 specularColor, float specularPower)
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

            mappedResource = context.MapSubresource(m_CameraBuffer, 0, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);
            CameraBufferType data2 = new CameraBufferType();
            data2.cameraPosition = cameraPosition;
            data2.padding = 0.0f;
            mappedResource.Data.Write<CameraBufferType>(data2);
            context.UnmapSubresource(m_CameraBuffer, 0);

            context.VertexShader.SetConstantBuffer(m_CameraBuffer, 1);

            mappedResource = context.MapSubresource(m_LightBuffer, 0, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);
            LightBufferType data3 = new LightBufferType();
            data3.ambientColor = ambientColor;
            data3.diffuseColor = diffuseColor;
            data3.lightDirection = lightDirection;
            data3.specularColor = specularColor;
            data3.specularPower = specularPower;
            mappedResource.Data.Write<LightBufferType>(data3);
            context.UnmapSubresource(m_LightBuffer, 0);

            context.PixelShader.SetConstantBuffer(m_LightBuffer, 0);

            return true;
        }

        public bool Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture, Vector3 lightDirection, Vector4 ambientColor, Vector4 diffuseColor, Vector3 cameraPosition, Vector4 specularColor, float specularPower)
        {
            if (!SetShaderParameters(context, worldMatrix, viewMatrix, projectionMatrix, texture, lightDirection, ambientColor, diffuseColor, cameraPosition, specularColor, specularPower))
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
