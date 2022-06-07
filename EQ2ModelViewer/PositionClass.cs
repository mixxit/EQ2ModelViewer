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
    public class PositionClass
    {
        private float m_PositionX;
        private float m_positionY;
        private float m_PositionZ;
        private float m_RotationX;
        private float m_RotationY;
        private float m_RotationZ;
        private float m_FrameTime;
        private float m_ForwardSpeed;
        private float m_BackwardSpeed;
        private float m_UpwardSpeed;
        private float m_DownwardSpeed;
        private float m_LeftTurnSpeed;
        private float m_RightTurnSpeed;
        private float m_LookUpSpeed;
        private float m_LookDownSpeed;
        public bool m_ShiftDown;
        public void SetPosition(float x, float y, float z)
        {
            m_PositionX = x;
            m_positionY = y;
            m_PositionZ = z;
        }

        public void SetRotation(float x, float y, float z)
        {
            m_RotationX = x;
            m_RotationY = y;
            m_RotationZ = z;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(m_PositionX, m_positionY, m_PositionZ);
        }

        public Vector3 GetRotation()
        {
            return new Vector3(m_RotationX, m_RotationY, m_RotationZ);
        }

        public void SetFrameTime(float time)
        {
            m_FrameTime = time;
        }

        public void MoveForward(bool keydown)
        {
            float radians;

            float mod = 1.0f;

            if (m_ShiftDown)
                mod = 10.0f;

            if (keydown)
            {
                m_ForwardSpeed += m_FrameTime * 0.001f * mod;
                if (m_ForwardSpeed > (m_FrameTime * 0.03f) * mod)
                    m_ForwardSpeed = m_FrameTime * 0.03f * mod;
            }
            else
            {
                m_ForwardSpeed -= m_FrameTime * 0.0007f * mod;
                if (m_ForwardSpeed < 0.0f)
                    m_ForwardSpeed = 0.0f;
            }
            radians = m_RotationY * 0.0174532925f;

            m_PositionX += (float)Math.Sin(radians) * m_ForwardSpeed;
            m_PositionZ += (float)Math.Cos(radians) * m_ForwardSpeed;
        }

        public void MoveBackward(bool keydown)
        {
            float radians;

            float mod = 1.0f;

            if (m_ShiftDown)
                mod = 10.0f;

            if (keydown)
            {
                m_BackwardSpeed += m_FrameTime * 0.001f * mod;
                if (m_BackwardSpeed > (m_FrameTime * 0.03f) * mod)
                    m_BackwardSpeed = m_FrameTime * 0.03f * mod;
            }
            else
            {
                m_BackwardSpeed -= m_FrameTime * 0.0007f * mod;
                if (m_BackwardSpeed < 0.0f)
                    m_BackwardSpeed = 0.0f;
            }
            radians = m_RotationY * 0.0174532925f;

            m_PositionX -= (float)Math.Sin(radians) * m_BackwardSpeed;
            m_PositionZ -= (float)Math.Cos(radians) * m_BackwardSpeed;
        }

        public void MoveUpward(bool keydown)
        {
            if (keydown)
            {
                m_UpwardSpeed += m_FrameTime * 0.003f;
                if (m_UpwardSpeed > (m_FrameTime * 0.03f))
                    m_UpwardSpeed = m_FrameTime * 0.03f;
            }
            else
            {
                m_UpwardSpeed -= m_FrameTime * 0.002f;
                if (m_UpwardSpeed < 0.0f)
                    m_UpwardSpeed = 0.0f;
            }

            m_positionY += m_UpwardSpeed;
        }

        public void MoveDownward(bool keydown)
        {
            if (keydown)
            {
                m_DownwardSpeed += m_FrameTime * 0.003f;
                if (m_DownwardSpeed > (m_FrameTime * 0.03f))
                    m_DownwardSpeed = m_FrameTime * 0.03f;
            }
            else
            {
                m_DownwardSpeed -= m_FrameTime * 0.002f;
                if (m_DownwardSpeed < 0.0f)
                    m_DownwardSpeed = 0.0f;
            }

            m_positionY -= m_DownwardSpeed;
        }

        public void TurnLeft(bool keydown)
        {
            // Update the left turn speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                m_LeftTurnSpeed += m_FrameTime * 0.01f;

                if (m_LeftTurnSpeed > (m_FrameTime * 0.15f))
                    m_LeftTurnSpeed = m_FrameTime * 0.15f;
            }
            else
            {
                m_LeftTurnSpeed -= m_FrameTime * 0.005f;

                if (m_LeftTurnSpeed < 0.0f)
                    m_LeftTurnSpeed = 0.0f;
            }

            // Update the rotation.
            m_RotationY -= m_LeftTurnSpeed;

            // Keep the rotation in the 0 to 360 range.
            if (m_RotationY < 0.0f)
                m_RotationY += 360.0f;
        }

        public void TurnRight(bool keydown)
        {
            // Update the right turn speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                m_RightTurnSpeed += m_FrameTime * 0.01f;

                if (m_RightTurnSpeed > (m_FrameTime * 0.15f))
                    m_RightTurnSpeed = m_FrameTime * 0.15f;
            }
            else
            {
                m_RightTurnSpeed -= m_FrameTime * 0.005f;

                if (m_RightTurnSpeed < 0.0f)
                    m_RightTurnSpeed = 0.0f;
            }

            // Update the rotation.
            m_RotationY += m_RightTurnSpeed;

            // Keep the rotation in the 0 to 360 range.
            if (m_RotationY > 360.0f)
                m_RotationY -= 360.0f;
        }

        public void LookUpward(bool keydown)
        {
            // Update the upward rotation speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                m_LookUpSpeed += m_FrameTime * 0.01f;

                if (m_LookUpSpeed > (m_FrameTime * 0.15f))
                    m_LookUpSpeed = m_FrameTime * 0.15f;
            }
            else
            {
                m_LookUpSpeed -= m_FrameTime * 0.005f;

                if (m_LookUpSpeed < 0.0f)
                    m_LookUpSpeed = 0.0f;
            }

            // Update the rotation.
            m_RotationX -= m_LookUpSpeed;

            // Keep the rotation maximum 90 degrees.
            if (m_RotationX > 90.0f)
                m_RotationX = 90.0f;
        }

        public void LookDownward(bool keydown)
        {
            // Update the downward rotation speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                m_LookDownSpeed += m_FrameTime * 0.01f;

                if (m_LookDownSpeed > (m_FrameTime * 0.15f))
                    m_LookDownSpeed = m_FrameTime * 0.15f;
            }
            else
            {
                m_LookDownSpeed -= m_FrameTime * 0.005f;

                if (m_LookDownSpeed < 0.0f)
                    m_LookDownSpeed = 0.0f;
            }

            // Update the rotation.
            m_RotationX += m_LookDownSpeed;

            // Keep the rotation maximum 90 degrees.
            if (m_RotationX < -90.0f)
                m_RotationX = -90.0f;
        }
    }
}
