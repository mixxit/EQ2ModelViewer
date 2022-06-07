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
    public class CameraClass
    {
        private float m_positionX;
        private float m_positionY;
        private float m_positionZ;
        private float m_rotationX;
        private float m_rotationY;
        private float m_rotationZ;
        private Matrix m_ViewMatrix;

        public void SetPosition(float x, float y, float z)
        {
            m_positionX = x;
            m_positionY = y;
            m_positionZ = z;
        }

        public void SetPosition(Vector3 pos)
        {
            SetPosition(pos.X, pos.Y, pos.Z);
        }

        public void SetRotation(float x, float y, float z)
        {
            m_rotationX = x;
            m_rotationY = y;
            m_rotationZ = z;
        }

        public void SetRotation(Vector3 rot)
        {
            SetRotation(rot.X, rot.Y, rot.Z);
        }

        public Vector3 GetPosition()
        {
            return new Vector3(m_positionX, m_positionY, m_positionZ);
        }

        public Vector3 GetRotation()
        {
            return new Vector3(m_rotationX, m_rotationY, m_rotationZ);
        }

        public void Render()
        {
            Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 pos = new Vector3(m_positionX, m_positionY, m_positionZ);
            Vector3 lookAt = new Vector3(0.0f, 0.0f, 1.0f);
            float pitch = m_rotationX * 0.0174532925f;
            float yaw = m_rotationY * 0.0174532925f;
            float roll = m_rotationZ * 0.0174532925f;
            Matrix rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);

            lookAt = Vector3.TransformCoordinate(lookAt, rotationMatrix);
            up = Vector3.TransformCoordinate(up, rotationMatrix);

            lookAt = pos + lookAt;
            m_ViewMatrix = Matrix.LookAtLH(pos, lookAt, up);
        }

        public Matrix GetViewMatrix()
        {
            return m_ViewMatrix;
        }
    }
}
