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
    public class GameObject
    {
        private struct YawPitchRoll
        {
            public float Yaw;
            public float Pitch;
            public float Roll;
        };

        public Vector3 Position = new Vector3(0.0f, 0.0f, 0.0f);
        private YawPitchRoll Rotation = new YawPitchRoll();

        GameObject()
        {
            Rotation.Yaw = 0.0f;
            Rotation.Pitch = 0.0f;
            Rotation.Roll = 0.0f;
        }

        private float m_scale;

        public float Scale
        {
            get { return m_scale; }
            set { m_scale = value; }
        }

        private UInt32 m_model;
        public UInt32 Model
        {
            get { return m_model; }
            set { m_model = value; }
        }
    }
}
