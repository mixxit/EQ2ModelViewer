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
    public class TextClass
    {
        public struct VertexType
        {
            public Vector3 position;
            public Vector2 texture;
        };

        private struct SentenceType
        {
            public Buffer vertexBuffer;
            public Buffer indexBuffer;
            public int vertexCount;
            public int indexCount;
            public int maxLength;
            public float red;
            public float green;
            public float blue;
        };

        private FontClass m_Font;
        private FontShaderClass m_FontShader;
        private int m_ScreenWidth;
        private int m_ScreenHeight;
        private Matrix m_BaseViewMatrix;

        private SentenceType m_Sentence1;
        private SentenceType m_Sentence2;
        private SentenceType m_Sentence3;
        private SentenceType m_Sentence4;
        private SentenceType m_Sentence5;
        private SentenceType m_Sentence6;
        private SentenceType m_Sentence7;
        private SentenceType m_Sentence8;
        private SentenceType m_Sentence9;
        private SentenceType m_Sentence10;
        private SentenceType m_Sentence11;

        public bool Initialize(Device device, DeviceContext context, int screenWidth, int screenHeight, Matrix baseViewMatrix)
        {
            m_ScreenWidth = screenWidth;
            m_ScreenHeight = screenHeight;
            m_BaseViewMatrix = baseViewMatrix;
            
            m_Font = new FontClass();
            if (!m_Font.Initialize(device, "fontdata.txt", "font.dds"))
                return false;

            m_FontShader = new FontShaderClass();
            if (!m_FontShader.Initialize(device))
                return false;

            InitializeSentence(ref m_Sentence1, 16, device);
            UpdateSentence(ref m_Sentence1, "Hello", 100, 100, 1.0f, 1.0f, 1.0f, context);

            InitializeSentence(ref m_Sentence2, 32, device);
            UpdateSentence(ref m_Sentence2, "Goodbye", 100, 200, 1.0f, 1.0f, 0.0f, context);

            InitializeSentence(ref m_Sentence3, 32, device);
            UpdateSentence(ref m_Sentence3, "", 0, 0, 1.0f, 1.0f, 0.0f, context);

            InitializeSentence(ref m_Sentence4, 32, device);
            UpdateSentence(ref m_Sentence4, "", 0, 0, 1.0f, 1.0f, 0.0f, context);

            InitializeSentence(ref m_Sentence5, 32, device);
            UpdateSentence(ref m_Sentence5, "", 0, 0, 1.0f, 1.0f, 0.0f, context);

            InitializeSentence(ref m_Sentence6, 32, device);
            UpdateSentence(ref m_Sentence6, "", 0, 0, 1.0f, 1.0f, 0.0f, context);

            InitializeSentence(ref m_Sentence7, 32, device);
            UpdateSentence(ref m_Sentence7, "", 0, 0, 1.0f, 1.0f, 0.0f, context);

            InitializeSentence(ref m_Sentence8, 32, device);
            UpdateSentence(ref m_Sentence8, "", 0, 0, 1.0f, 1.0f, 0.0f, context);

            InitializeSentence(ref m_Sentence9, 32, device);
            UpdateSentence(ref m_Sentence9, "", 0, 0, 1.0f, 1.0f, 0.0f, context);

            InitializeSentence(ref m_Sentence10, 32, device);
            UpdateSentence(ref m_Sentence10, "", 0, 0, 1.0f, 1.0f, 0.0f, context);

            InitializeSentence(ref m_Sentence11, 32, device);
            UpdateSentence(ref m_Sentence11, "", 0, 0, 1.0f, 1.0f, 0.0f, context);
            return true;
        }

        public void ShutDown()
        {
            ReleaseSentence(ref m_Sentence1);
            ReleaseSentence(ref m_Sentence2);
            ReleaseSentence(ref m_Sentence3);
            ReleaseSentence(ref m_Sentence4);
            ReleaseSentence(ref m_Sentence5);
            ReleaseSentence(ref m_Sentence6);
            ReleaseSentence(ref m_Sentence7);
            ReleaseSentence(ref m_Sentence8);
            ReleaseSentence(ref m_Sentence9);
            ReleaseSentence(ref m_Sentence10);
            ReleaseSentence(ref m_Sentence11);

            if (m_FontShader != null)
            {
                m_FontShader.ShutDown();
                m_FontShader = null;
            }

            if (m_Font != null)
            {
                m_Font.ShutDown();
                m_Font = null;
            }
        }

        public bool Render(DeviceContext context, Matrix world, Matrix ortho)
        {
            RenderSentence(context, m_Sentence1, world, ortho);
            RenderSentence(context, m_Sentence2, world, ortho);
            RenderSentence(context, m_Sentence3, world, ortho);
            RenderSentence(context, m_Sentence4, world, ortho);
            RenderSentence(context, m_Sentence5, world, ortho);
            RenderSentence(context, m_Sentence6, world, ortho);
            RenderSentence(context, m_Sentence7, world, ortho);
            RenderSentence(context, m_Sentence8, world, ortho);
            RenderSentence(context, m_Sentence9, world, ortho);
            RenderSentence(context, m_Sentence10, world, ortho);
            RenderSentence(context, m_Sentence11, world, ortho);

            return true;
        }

        private bool InitializeSentence(ref SentenceType sentence, int maxLength, Device device)
        {
            BufferDescription vertexBufferDesc;
            BufferDescription indexBufferDesc;
            DataStream vertexData;
            DataStream indexData;

            sentence = new SentenceType();
            sentence.maxLength = maxLength;
            sentence.vertexCount = 6 * maxLength;
            sentence.indexCount = sentence.vertexCount;
            
            vertexData = new DataStream(sentence.vertexCount * System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertexType)), true, true);
            indexData = new DataStream(sentence.indexCount * sizeof(uint), true, true);

            for (uint i = 0; i < sentence.indexCount; i++)
                indexData.Write(i);

            indexData.Position = 0;
            
            vertexBufferDesc = new BufferDescription(System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertexType)) * sentence.vertexCount, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            sentence.vertexBuffer = new Buffer(device, vertexData, vertexBufferDesc);

            indexBufferDesc = new BufferDescription(System.Runtime.InteropServices.Marshal.SizeOf(typeof(UInt32)) * sentence.indexCount, ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            sentence.indexBuffer = new Buffer(device, indexData, indexBufferDesc);
            return true;
        }

        private bool UpdateSentence(ref SentenceType sentence, string text, int positionX, int positionY, float red, float green, float blue, DeviceContext context)
        {
            int numLetters;
            VertexType[] vertices;
            float drawX;
            float drawY;

            sentence.red = red;
            sentence.green = green;
            sentence.blue = blue;

            numLetters = text.Length;
            if (numLetters > sentence.maxLength)
                return false;

            vertices = new VertexType[sentence.vertexCount];

            drawX = (float)((m_ScreenWidth / 2 * -1) + positionX);
            drawY = (float)((m_ScreenHeight / 2) - positionY);

            m_Font.BuildVertexArray(text, drawX, drawY, ref vertices);

            var mappedResource = context.MapSubresource(sentence.vertexBuffer, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);
            VertexType[] data = vertices;
            mappedResource.Data.WriteRange<VertexType>(data);
            context.UnmapSubresource(sentence.vertexBuffer, 0);

            return true;
        }

        private void ReleaseSentence(ref SentenceType sentence)
        {
            if (sentence.vertexBuffer != null)
                sentence.vertexBuffer.Dispose();
            if (sentence.indexBuffer != null)
                sentence.indexBuffer.Dispose();
        }

        private bool RenderSentence(DeviceContext context, SentenceType sentence, Matrix world, Matrix ortho)
        {
            int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertexType));
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(sentence.vertexBuffer, stride, 0));
            context.InputAssembler.SetIndexBuffer(sentence.indexBuffer, Format.R32_UInt, 0);
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            Vector4 pixelColor = new Vector4(sentence.red, sentence.green, sentence.blue, 1.0f);

            if (!m_FontShader.Render(context, sentence.indexCount, world, m_BaseViewMatrix, ortho, m_Font.GetTexture(), pixelColor))
                return false;

            return true;
        }

        public bool SetFPS(int fps, DeviceContext context)
        {
            float red;
            float green;
            float blue;

            if (fps > 9999)
                fps = 9999;

            if (fps >= 60)
            {
                red = 0.0f;
                green = 1.0f;
                blue = 0.0f;
            }
            else if (fps > 30)
            {
                red = 1.0f;
                green = 1.0f;
                blue = 0.0f;
            }
            else
            {
                red = 1.0f;
                green = 0.0f;
                blue = 0.0f;
            }

            if (!UpdateSentence(ref m_Sentence1, "FPS: " + fps.ToString(), 20, 20, red, green, blue, context))
                return false;

            return true;
        }

        public bool SetPosition(Vector3 position, DeviceContext context)
        {
            if (!UpdateSentence(ref m_Sentence10, "Loc: " + position.ToString(), 20, 40, 0.0f, 1.0f, 0.0f, context))
                return false;

            return true;
        }

        public bool SetSelectedModel(Model model, DeviceContext context) {
            bool ret;
            if (model != null) {
                ret = UpdateSentence(ref m_Sentence2, "Selected Model Info:", 20, 60, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence3, "Widget ID: " + model.WidgetID.ToString(), 20, 80, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence4, "X: " + model.Position.X.ToString(), 20, 100, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence5, "Y: " + model.Position.Y.ToString(), 20, 120, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence6, "Z: " + model.Position.Z.ToString(), 20, 140, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence7, "Yaw: " + model.Rotation.X.ToString(), 20, 160, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence8, "Pitch: " + model.Rotation.Y.ToString(), 20, 180, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence9, "Roll: " + model.Rotation.Z.ToString(), 20, 200, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence11, "Grid: " + model.GridID.ToString(), 20, 220, 1.0f, 0.0f, 0.0f, context);
            }
            else {
                ret = UpdateSentence(ref m_Sentence2, "", 20, 60, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence3, "", 20, 80, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence4, "", 20, 100, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence5, "", 20, 120, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence6, "", 20, 140, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence7, "", 20, 160, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence8, "", 20, 180, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence9, "", 20, 200, 1.0f, 0.0f, 0.0f, context)
                    && UpdateSentence(ref m_Sentence11, "", 20, 220, 1.0f, 0.0f, 0.0f, context);
            }

            return ret;
        }
    }
}
