using System;
using System.Collections.Generic;

using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using Buffer = SlimDX.Direct3D11.Buffer;


namespace EQ2ModelViewer {
    public class BitmapClass {

        public struct VertexType {
            public Vector3 position;
            public Vector2 texture;
        };

        Buffer m_vertexBuffer;
        Buffer m_indexBuffer;
        TextureClass m_texture;
        int m_vertexCount;
        int m_indexCount;
        int m_screenWidth;
        int m_screenHeight;
        int m_bmpWidth;
        int m_bmpHeight;
        int m_prevPosX;
        int m_prevPosY;
        TextureShaderClass m_textureShader;
        private Matrix m_BaseViewMatrix;

        public bool Initialize(Device device, int screenWidth, int screenHeight, string filename, int bmpWidth, int bmpHeight, Matrix baseViewMatrix) {
            // Store screen size
            m_screenWidth = screenWidth;
            m_screenHeight = screenHeight;

            // Store the size in pixels that this bitmap should be rendered at
            m_bmpWidth = bmpWidth;
            m_bmpHeight = bmpHeight;

            // Init previous rendering position to -1
            m_prevPosX = -1;
            m_prevPosY = -1;

            m_BaseViewMatrix = baseViewMatrix;

            // Init the vertex and index buffers
            if (!InitializeBuffers(device))
                return false;

            if (!LoadTexture(device, filename))
                return false;

            m_textureShader = new TextureShaderClass();
            if (!m_textureShader.Initialize(device))
                return false;

            return true;
        }

        public void ShutDown() {
            if (m_indexBuffer != null)
                m_indexBuffer.Dispose();
            if (m_vertexBuffer != null)
                m_vertexBuffer.Dispose();
            if (m_texture != null)
                m_texture.ShutDown();
            if (m_textureShader != null)
                m_textureShader.ShutDown();
        }

        public bool Render(GraphicClass Graphics, int posX, int posY) {
            if (!UpdateBuffers(Graphics.Context, posX, posY))
                return false;

            RenderBuffers(Graphics);

            return true;
        }

        public int GetIndexCount() {
            return m_indexCount;
        }

        public ShaderResourceView GetTexture() {
            return m_texture.GetTexture();
        }

        private bool InitializeBuffers(Device device) {
            BufferDescription vertexBufferDesc;
            BufferDescription indexBufferDesc;
            DataStream vertexData;
            DataStream indexData;

            m_vertexCount = 6;
            m_indexCount = m_vertexCount;

            vertexData = new DataStream(m_vertexCount * System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertexType)), true, true);
            indexData = new DataStream(m_indexCount * sizeof(uint), true, true);

            for (uint i = 0; i < m_indexCount; i++)
                indexData.Write(i);

            indexData.Position = 0;

            vertexBufferDesc = new BufferDescription(System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertexType)) * m_vertexCount, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            m_vertexBuffer = new Buffer(device, vertexData, vertexBufferDesc);

            indexBufferDesc = new BufferDescription(System.Runtime.InteropServices.Marshal.SizeOf(typeof(UInt32)) * m_indexCount, ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            m_indexBuffer = new Buffer(device, indexData, indexBufferDesc);

            return true;
        }

        private bool UpdateBuffers(DeviceContext context, int posX, int posY) {
            float left;
            float right;
            float top;
            float bottom;
            VertexType[] vertices;
            DataBox mappedResource;

            if (posX == m_prevPosX && posY == m_prevPosY)
                return true;

            m_prevPosX = posX;
            m_prevPosY = posY;

            // Calculate the screen coordinate of the left side of the bitmap
            left = (float)((m_screenWidth / 2) * -1) + (float)posX;
            right = left + (float)m_bmpWidth;

            // Calculate the screen coordinates of the top of the bitmap
            top = (float)(m_screenHeight / 2) - (float)posY;
            bottom = top - (float)m_bmpHeight;

            vertices = new VertexType[m_vertexCount];
            vertices[0].position = new Vector3(left, top, 0.0f);
            vertices[0].texture = new Vector2(0.0f, 0.0f);

            vertices[1].position = new Vector3(right, bottom, 0.0f);
            vertices[1].texture = new Vector2(1.0f, 1.0f);

            vertices[2].position = new Vector3(left, bottom, 0.0f);
            vertices[2].texture = new Vector2(0.0f, 1.0f);

            vertices[3].position = new Vector3(left, top, 0.0f);
            vertices[3].texture = new Vector2(0.0f, 0.0f);

            vertices[4].position = new Vector3(right, top, 0.0f);
            vertices[4].texture = new Vector2(1.0f, 0.0f);

            vertices[5].position = new Vector3(right, bottom, 0.0f);
            vertices[5].texture = new Vector2(1.0f, 1.0f);

            mappedResource = context.MapSubresource(m_vertexBuffer, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);
            VertexType[] data = vertices;
            mappedResource.Data.WriteRange<VertexType>(data);
            context.UnmapSubresource(m_vertexBuffer, 0);

            return true;
        }

        private void RenderBuffers(GraphicClass Graphics) {
            int stride;
            int offset;

            stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertexType));
            offset = 0;

            Graphics.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vertexBuffer, stride, offset));
            Graphics.Context.InputAssembler.SetIndexBuffer(m_indexBuffer, Format.R32_UInt, offset);
            Graphics.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            m_textureShader.Render(Graphics.Context, m_indexCount, Graphics.GetWorldMatrix(), m_BaseViewMatrix, Graphics.GetOrthoMatrix(), GetTexture());
        }

        private bool LoadTexture(Device device, string filename) {
            m_texture = new TextureClass();
            if (!m_texture.Initialize(device, filename))
                return false;

            return true;
        }
    }
}
