using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

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
    public class MeshClass
    {
        public struct EQ2Model
        {
            public float x, y, z;
            public float tu, tv;
            public float nx, ny, nz;
        }
        public struct EQ2Region
        {
            public float x, y, z;
            public float tu, tv, tz;
        }

        private Buffer m_VertexBuffer;
        private Buffer m_IndexBuffer;
        private int m_VertexCount;
        private int m_IndexCount;
        public EQ2Model[] m_model;
        TextureClass m_Texture;

        public void SetFaceCount(int count)
        {
            m_VertexCount = count;
            m_IndexCount = count;
            m_model = new EQ2Model[count];
        }

        public void AddData(int index, float x, float y, float z, float nx, float ny, float nz, float tu, float tv)
        {
            if (index > m_VertexCount)
                throw new IndexOutOfRangeException("MeshClass AddData Failed: index (" + index + ") is greater then m_VertexCount (" + m_VertexCount + ")");

            m_model[index].x = x;
            m_model[index].y = y;
            m_model[index].z = z;

            m_model[index].nx = nx;
            m_model[index].ny = ny;
            m_model[index].nz = nz;

            m_model[index].tu = tu;
            m_model[index].tv = tv;
        }

        public List<Vector3> GetVertices()
        {
            List<Vector3> ret = new List<Vector3>();
            foreach (EQ2Model m in m_model)
            {
                Vector3 newVec = new Vector3(m.x, m.y, m.z);
                ret.Add(newVec);
            }

            return ret;
        }

        public bool InitializeBuffers(Device device)
        {
            BufferDescription vertexBufferDesc = new BufferDescription();
            BufferDescription indexBufferDesc = new BufferDescription();
            DataStream vertices = new DataStream(System.Runtime.InteropServices.Marshal.SizeOf(typeof(EQ2Model)) * m_VertexCount, true, true);
            DataStream indices = new DataStream(sizeof(ulong) * m_IndexCount, true, true);

            for (int i = 0; i < m_VertexCount; i++)
            {
                vertices.Write(new Vector3(m_model[i].x, m_model[i].y, m_model[i].z));
                vertices.Write(new Vector2(m_model[i].tu, m_model[i].tv));
                vertices.Write(new Vector3(m_model[i].nx, m_model[i].ny, m_model[i].nz));
                indices.Write(i);
            }
            vertices.Position = 0;
            indices.Position = 0;

            vertexBufferDesc.Usage = ResourceUsage.Default;
            vertexBufferDesc.SizeInBytes = (int)vertices.Length;
            vertexBufferDesc.BindFlags = BindFlags.VertexBuffer;
            vertexBufferDesc.CpuAccessFlags = CpuAccessFlags.None;
            vertexBufferDesc.OptionFlags = ResourceOptionFlags.None;
            vertexBufferDesc.StructureByteStride = 0;


            m_VertexBuffer = new Buffer(device, vertices, vertexBufferDesc);
            vertices.Dispose();

            indexBufferDesc.Usage = ResourceUsage.Default;
            indexBufferDesc.SizeInBytes = (int)indices.Length;
            indexBufferDesc.BindFlags = BindFlags.IndexBuffer;
            indexBufferDesc.CpuAccessFlags = CpuAccessFlags.None;
            indexBufferDesc.OptionFlags = ResourceOptionFlags.None;
            indexBufferDesc.StructureByteStride = 0;

            m_IndexBuffer = new Buffer(device, indices, indexBufferDesc);
            indices.Dispose();
            return true;
        }

        public void RenderBuffers(DeviceContext context)
        {
            int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(EQ2Model));
            int offset = 0;

            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_VertexBuffer, stride, offset));
            context.InputAssembler.SetIndexBuffer(m_IndexBuffer, Format.R32_UInt, 0);
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }

        public void ShutDown()
        {
            ReleaseTexture();
            m_VertexBuffer.Dispose();
            m_IndexBuffer.Dispose();
        }

        public int GetIndexCount()
        {
            return m_IndexCount;
        }

        public ShaderResourceView GetTexture()
        {
            if (m_Texture == null)
                return null;

            return m_Texture.GetTexture();
        }

        private void ReleaseTexture()
        {
            if (m_Texture != null)
                m_Texture.ShutDown();
        }

        public bool LoadTexture(Device device, string filename)
        {
            m_Texture = new TextureClass();
            if (m_Texture == null)
            {
                Console.WriteLine("Model: Failed to create an instance of TextureClass()...WTF!");
                return false;
            }

            if (!m_Texture.Initialize(device, filename))
            {
                Console.WriteLine("Model: Failed to load the texture.");
                return false;
            }

            return true;
        }
    }
}
