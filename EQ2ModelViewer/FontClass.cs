using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

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
    public class FontClass
    {
        private struct FontType
        {
            public float left;
            public float right;
            public int size;
        }

        private FontType[] m_font;
        private TextureClass m_texture;

        public bool Initialize(Device device, string fontFile, string textureFile)
        {
            if (!LoadFontData(fontFile))
                return false;

            if (!LoadTexture(device, textureFile))
                return false;

            return true;
        }

        public void ShutDown()
        {
            ReleaseTexture();
        }

        public ShaderResourceView GetTexture()
        {
            return m_texture.GetTexture();
        }

        public void BuildVertexArray(string sentence, float drawX, float drawY, ref TextClass.VertexType[] vertices)
        {
            int numLetters;
            int index;
            int i;
            int letter;

            numLetters = sentence.Length;
            index = 0;
            for (i = 0; i < numLetters; i++)
            {
                letter = ((int)sentence[i]) - 32;
                if (letter == 0)
                {
                    drawX += 3.0f;
                }
                else
                {
                    // First Triangle
                    // Top Left
                    vertices[index].position = new Vector3(drawX, drawY, 0.0f);
                    vertices[index].texture = new Vector2(m_font[letter].left, 0.0f);
                    index++;

                    // Bottom Right
                    vertices[index].position = new Vector3(drawX + m_font[letter].size, (drawY - 16), 0.0f);
                    vertices[index].texture = new Vector2(m_font[letter].right, 1.0f);
                    index++;

                    // Bottom Left
                    vertices[index].position = new Vector3(drawX, (drawY - 16), 0.0f);
                    vertices[index].texture = new Vector2(m_font[letter].left, 1.0f);
                    index++;

                    // Second Triangle
                    // Top Left
                    vertices[index].position = new Vector3(drawX, drawY, 0.0f);
                    vertices[index].texture = new Vector2(m_font[letter].left, 0.0f);
                    index++;

                    // Top Right
                    vertices[index].position = new Vector3(drawX + m_font[letter].size, drawY, 0.0f);
                    vertices[index].texture = new Vector2(m_font[letter].right, 0.0f);
                    index++;

                    // Bottom right
                    vertices[index].position = new Vector3((drawX + m_font[letter].size), (drawY - 16), 0.0f);
                    vertices[index].texture = new Vector2(m_font[letter].right, 1.0f);
                    index++;

                    // Update the x location for drawing by the size of the letter and one pixel
                    drawX += m_font[letter].size + 1.0f;
                }
            }
        }

        private bool LoadFontData(string fontFile)
        {
            string line;
            byte i = 0;
            m_font = new FontType[95];
            StreamReader reader = new StreamReader(File.Open(fontFile, FileMode.Open));
            Regex trimmer = new Regex(@"([0-9]+)\s.{1}\s([0-9\.]+)\s+([0-9\.]+)\s+([0-9\.]+)");
            while (i < 95)
            {
                line = reader.ReadLine();
                Match out_ = trimmer.Match(line);
                if (!out_.Success)
                    continue;

                m_font[i].left = float.Parse(out_.Groups[2].Value);

                m_font[i].right = float.Parse(out_.Groups[3].Value);

                m_font[i].size = int.Parse(out_.Groups[4].Value);

                i++;
            }
            reader.Close();

            return true;
        }

        private bool LoadTexture(Device device, string textureFile)
        {
            m_texture = new TextureClass();
            m_texture.Initialize(device, textureFile);
            return true;
        }

        private void ReleaseTexture()
        {
            if (m_texture != null)
                m_texture.ShutDown();
        }
    }
}
