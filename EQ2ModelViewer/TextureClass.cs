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
    public class TextureClass
    {
        private ShaderResourceView m_Texture;

        public bool Initialize(Device device, string fileName)
        {
            try
            {
                m_Texture = ShaderResourceView.FromFile(device, fileName);
            }
            catch
            {
                m_Texture = ShaderResourceView.FromFile(device, "goblin_ice.dds");
            }
            if (m_Texture == null)
            {
                Console.WriteLine("TextureClass: Unable to load texture (" + fileName + ")");
                return false;
            }
            return true;
        }

        public ShaderResourceView GetTexture()
        {
            return m_Texture;
        }

        public void ShutDown()
        {
            m_Texture.Dispose();
        }
    }
}
