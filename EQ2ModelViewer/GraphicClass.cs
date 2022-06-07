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
    public class GraphicClass
    {
        private Device m_device;
        private DeviceContext m_context;
        private SwapChain m_swapChain;
        private RenderTargetView m_renderTarget;
        private Texture2D m_depthBuffer;
        private DepthStencilState m_depthStencilState;
        private DepthStencilState m_depthDisabledStencilState;
        private DepthStencilView m_depthStencilView;
        private RasterizerState m_rasterState;

        private BlendState m_alphaEnabledBlendState;
        private BlendState m_alphaDisableBlendState;

        private Matrix m_worldMatrix;
        private Matrix m_orthoMatrix;
        private Matrix m_projectionMatrix;

        public Device Device
        {
            get { return m_device; }
        }

        public DeviceContext Context
        {
            get { return m_context; }
        }

        public SwapChain SwapChain
        {
            get { return m_swapChain; }
        }

        public Matrix GetWorldMatrix()
        {
            return m_worldMatrix;
        }

        public Matrix GetOrthoMatrix()
        {
            return m_orthoMatrix;
        }

        public Matrix GetProjectionMatrix()
        {
            return m_projectionMatrix;
        }

        public bool Initialize(Panel pGraphics)
        {
            SwapChainDescription description = new SwapChainDescription();

            ModeDescription modedesc = new ModeDescription(pGraphics.ClientSize.Width, pGraphics.ClientSize.Height, new Rational(0, 1), Format.R8G8B8A8_UNorm);
            modedesc.ScanlineOrdering = DisplayModeScanlineOrdering.Unspecified;
            modedesc.Scaling = DisplayModeScaling.Unspecified;
            description.BufferCount = 1;
            description.IsWindowed = true;
            description.ModeDescription = modedesc;
            description.Usage = Usage.RenderTargetOutput;
            description.OutputHandle = pGraphics.Handle;
            description.SampleDescription = new SampleDescription(1, 0);
            description.Flags = 0;

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, description, out m_device, out m_swapChain);
            m_context = m_device.ImmediateContext;

            using (var resource = Resource.FromSwapChain<Texture2D>(m_swapChain, 0))
                m_renderTarget = new RenderTargetView(m_device, resource);

            Texture2DDescription depthBufferDesc = new Texture2DDescription();
            depthBufferDesc.Width = pGraphics.ClientSize.Width;
            depthBufferDesc.Height = pGraphics.ClientSize.Height;
            depthBufferDesc.MipLevels = 1;
            depthBufferDesc.ArraySize = 1;
            depthBufferDesc.Format = Format.D24_UNorm_S8_UInt;
            depthBufferDesc.SampleDescription = new SampleDescription(1, 0);
            depthBufferDesc.Usage = ResourceUsage.Default;
            depthBufferDesc.BindFlags = BindFlags.DepthStencil;
            depthBufferDesc.CpuAccessFlags = CpuAccessFlags.None;
            depthBufferDesc.OptionFlags = ResourceOptionFlags.None;
            m_depthBuffer = new Texture2D(m_device, depthBufferDesc);

            DepthStencilStateDescription depthStencilStateDesc = new DepthStencilStateDescription();
            depthStencilStateDesc.IsDepthEnabled = true;
            depthStencilStateDesc.DepthWriteMask = DepthWriteMask.All;
            depthStencilStateDesc.DepthComparison = Comparison.Less;

            depthStencilStateDesc.IsStencilEnabled = true;
            depthStencilStateDesc.StencilReadMask = 0xFF;
            depthStencilStateDesc.StencilWriteMask = 0xFF;

            DepthStencilOperationDescription frontface = new DepthStencilOperationDescription();
            frontface.FailOperation = StencilOperation.Keep;
            frontface.DepthFailOperation = StencilOperation.Increment;
            frontface.PassOperation = StencilOperation.Keep;
            frontface.Comparison = Comparison.Always;
            depthStencilStateDesc.FrontFace = frontface;

            DepthStencilOperationDescription backface = new DepthStencilOperationDescription();
            backface.FailOperation = StencilOperation.Keep;
            backface.DepthFailOperation = StencilOperation.Decrement;
            backface.PassOperation = StencilOperation.Keep;
            backface.Comparison = Comparison.Always;
            depthStencilStateDesc.BackFace = backface;

            m_depthStencilState = DepthStencilState.FromDescription(m_device, depthStencilStateDesc);

            m_context.OutputMerger.DepthStencilState = m_depthStencilState;
            m_context.OutputMerger.DepthStencilReference = 1;

            DepthStencilViewDescription depthStencilViewDesc = new DepthStencilViewDescription();
            depthStencilViewDesc.Format = Format.D24_UNorm_S8_UInt;
            depthStencilViewDesc.Dimension = DepthStencilViewDimension.Texture2D;
            depthStencilViewDesc.MipSlice = 0;

            m_depthStencilView = new DepthStencilView(m_device, m_depthBuffer, depthStencilViewDesc);
            m_context.OutputMerger.SetTargets(m_depthStencilView, m_renderTarget);

            RasterizerStateDescription rasterDesc = new RasterizerStateDescription();
            rasterDesc.IsAntialiasedLineEnabled = false;
            rasterDesc.CullMode = CullMode.Back;
            rasterDesc.DepthBias = 0;
            rasterDesc.DepthBiasClamp = 0.0f;
            rasterDesc.IsDepthClipEnabled = true;
            rasterDesc.FillMode = FillMode.Solid;
            rasterDesc.IsFrontCounterclockwise = false;
            rasterDesc.IsMultisampleEnabled = false;
            rasterDesc.IsScissorEnabled = false;
            rasterDesc.SlopeScaledDepthBias = 0.0f;

            m_rasterState = RasterizerState.FromDescription(m_device, rasterDesc);
            m_context.Rasterizer.State = m_rasterState;

            var viewport = new Viewport(0.0f, 0.0f, pGraphics.ClientSize.Width, pGraphics.ClientSize.Height, 0.0f, 1.0f);
            m_context.Rasterizer.SetViewports(viewport);

            using (var factory = m_swapChain.GetParent<Factory>())
                factory.SetWindowAssociation(pGraphics.Handle, WindowAssociationFlags.IgnoreAltEnter);

            // World matrix
            m_worldMatrix = Matrix.Identity;

            // Projection matrix            
            float fieldOfView = (float)Math.PI / 4.0f;
            float screenAspect = (float)viewport.Width / viewport.Height;
            m_projectionMatrix = Matrix.PerspectiveFovLH(fieldOfView, screenAspect, 0.1f, 1000.0f);
            
            // Ortho matrix
            m_orthoMatrix = Matrix.OrthoLH(viewport.Width, viewport.Height, 0.1f, 1000.0f);

            DepthStencilStateDescription depthDisabledDesc = new DepthStencilStateDescription();
            depthDisabledDesc.IsDepthEnabled = false;
            depthDisabledDesc.DepthWriteMask = DepthWriteMask.All;
            depthDisabledDesc.DepthComparison = Comparison.Less;
            depthDisabledDesc.IsStencilEnabled = true;
            depthDisabledDesc.StencilReadMask = 0xFF;
            depthDisabledDesc.StencilWriteMask = 0xFF;
            depthDisabledDesc.FrontFace = frontface;
            depthDisabledDesc.BackFace = backface;

            m_depthDisabledStencilState = DepthStencilState.FromDescription(m_device, depthDisabledDesc);

            BlendStateDescription blendDesc = new BlendStateDescription();
            blendDesc.RenderTargets[0].BlendEnable = true;
            blendDesc.RenderTargets[0].SourceBlend = BlendOption.One;
            blendDesc.RenderTargets[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            blendDesc.RenderTargets[0].BlendOperation = BlendOperation.Add;
            blendDesc.RenderTargets[0].SourceBlendAlpha = BlendOption.One;
            blendDesc.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
            blendDesc.RenderTargets[0].BlendOperationAlpha = BlendOperation.Add;
            blendDesc.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            m_alphaEnabledBlendState = BlendState.FromDescription(m_device, blendDesc);

            blendDesc.RenderTargets[0].BlendEnable = false;
            m_alphaDisableBlendState = BlendState.FromDescription(m_device, blendDesc);

            /*
            BlendStateDescription blendDesc2 = new BlendStateDescription();
            blendDesc2.RenderTargets[0].BlendEnable = false;
            blendDesc2.RenderTargets[0].SourceBlend = BlendOption.One;
            blendDesc2.RenderTargets[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            blendDesc2.RenderTargets[0].BlendOperation = BlendOperation.Add;
            blendDesc2.RenderTargets[0].SourceBlendAlpha = BlendOption.One;
            blendDesc2.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
            blendDesc2.RenderTargets[0].BlendOperationAlpha = BlendOperation.Add;
            blendDesc2.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            m_alphaDisableBlendState = BlendState.FromDescription(m_device, blendDesc2);
            */

            return true;
        }

        public void ShutDown()
        {
            if (m_alphaEnabledBlendState != null)
                m_alphaEnabledBlendState.Dispose();
            if (m_alphaDisableBlendState != null)
                m_alphaDisableBlendState.Dispose();
            if (m_rasterState != null)
                m_rasterState.Dispose();
            if (m_depthStencilView != null)
                m_depthStencilView.Dispose();
            if (m_depthDisabledStencilState != null)
                m_depthDisabledStencilState.Dispose();
            if (m_depthStencilState != null)
                m_depthStencilState.Dispose();
            if (m_depthBuffer != null)
                m_depthBuffer.Dispose();
            if (m_renderTarget != null)
                m_renderTarget.Dispose();
            if (m_swapChain != null)
                m_swapChain.Dispose();
            if (m_device != null)
                m_device.Dispose();
        }

        public void BeginScene()
        {
            m_context.ClearRenderTargetView(m_renderTarget, new Color4(0.0f, 0.0f, 0.0f));
            m_context.ClearDepthStencilView(m_depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        public void EndScene()
        {
            m_swapChain.Present(0, PresentFlags.None);
        }

        public void TurnZBufferOn()
        {
            m_context.OutputMerger.DepthStencilState = m_depthStencilState;
            m_context.OutputMerger.DepthStencilReference = 1;
        }

        public void TurnZBufferOff()
        {
            m_context.OutputMerger.DepthStencilState = m_depthDisabledStencilState;
            m_context.OutputMerger.DepthStencilReference = 1;
        }

        public void TurnOnAlphaBlending()
        {
            m_context.OutputMerger.BlendState = m_alphaEnabledBlendState;
            m_context.OutputMerger.BlendFactor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        }

        public void TurnOffAlphaBlending()
        {
            m_context.OutputMerger.BlendState = m_alphaDisableBlendState;
            m_context.OutputMerger.BlendFactor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        }
    }
}
