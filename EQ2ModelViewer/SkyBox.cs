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
    public class SkyBox
    {
        int NumSphereVertices = 0;
        int NumSphereFaces = 0;

        private void CreateSphere(int LatLines, int LongLines)
        {
            NumSphereVertices = ((LatLines - 2) * LongLines) + 2;
            NumSphereFaces = ((LatLines - 3) * LongLines * 2) + (LongLines * 2);

            float sphereYaw = 0.0f;
            float spherePitch = 0.0f;

            /*
            DataStream vertices = new DataStream(System.Runtime.InteropServices.Marshal.SizeOf(typeof(EQ2Model)) * m_VertexCount, true, true);
            Vector4 currVertPos = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);

            SlimDX.Direct3D11.Devic
            */
        }
    }
}
