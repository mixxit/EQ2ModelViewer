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
    public class FrustumClass
    {
        private Plane[] m_Planes = new Plane[6];

        public void ConstructFrustum(float screenDepth, Matrix projectionMatrix, Matrix viewMatrix)
        {
            float zMin;
            float r;
            Matrix matrix;

            zMin = -projectionMatrix.M43 / projectionMatrix.M33;
            r = screenDepth / (screenDepth - zMin);
            projectionMatrix.M33 = r;
            projectionMatrix.M43 = -r * zMin;

            matrix = Matrix.Multiply(viewMatrix, projectionMatrix);

            float a = matrix.M14 + matrix.M13;
            float b = matrix.M24 + matrix.M23;
            float c = matrix.M34 + matrix.M33;
            float d = matrix.M44 + matrix.M43;
            m_Planes[0] = new Plane(a, b, c, d);
            m_Planes[0].Normalize();

            a = matrix.M14 - matrix.M13;
            b = matrix.M24 - matrix.M23;
            c = matrix.M34 - matrix.M33;
            d = matrix.M44 - matrix.M43;
            m_Planes[1] = new Plane(a, b, c, d);
            m_Planes[1].Normalize();

            a = matrix.M14 + matrix.M11;
            b = matrix.M24 + matrix.M21;
            c = matrix.M34 + matrix.M31;
            d = matrix.M44 + matrix.M41;
            m_Planes[2] = new Plane(a, b, c, d);
            m_Planes[2].Normalize();

            a = matrix.M14 - matrix.M11;
            b = matrix.M24 - matrix.M21;
            c = matrix.M34 - matrix.M31;
            d = matrix.M44 - matrix.M41;
            m_Planes[3] = new Plane(a, b, c, d);
            m_Planes[3].Normalize();

            a = matrix.M14 - matrix.M12;
            b = matrix.M24 - matrix.M22;
            c = matrix.M34 - matrix.M32;
            d = matrix.M44 - matrix.M42;
            m_Planes[4] = new Plane(a, b, c, d);
            m_Planes[4].Normalize();

            a = matrix.M14 + matrix.M12;
            b = matrix.M24 + matrix.M22;
            c = matrix.M34 + matrix.M32;
            d = matrix.M44 + matrix.M42;
            m_Planes[5] = new Plane(a, b, c, d);
            m_Planes[5].Normalize();
        }

        public bool CheckPoint(float x, float y, float z)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Plane.DotCoordinate(m_Planes[i], new Vector3(x, y, z)) < 0.0f)
                    return false;
            }

            return true;
        }

        public bool CheckCube(float xCenter, float yCenter, float zCenter, float radius)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter - radius), (yCenter - radius), (zCenter - radius))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter + radius), (yCenter - radius), (zCenter - radius))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter - radius), (yCenter + radius), (zCenter - radius))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter + radius), (yCenter + radius), (zCenter - radius))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter - radius), (yCenter - radius), (zCenter + radius))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter + radius), (yCenter - radius), (zCenter + radius))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter - radius), (yCenter + radius), (zCenter + radius))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter + radius), (yCenter + radius), (zCenter + radius))) >= 0.0f)
                    continue;

                return false;
            }

            return true;
        }

        public bool CheckSphere(float xCenter, float yCenter, float zCenter, float radius)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Plane.DotCoordinate(m_Planes[i], new Vector3(xCenter, yCenter, zCenter)) < -radius)
                    return false;
            }

            return true;
        }

        public bool CheckRectangle(float xCenter, float yCenter, float zCenter, float xSize, float ySize, float zSize)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter - xSize), (yCenter - ySize), (zCenter - zSize))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter + xSize), (yCenter - ySize), (zCenter - zSize))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter - xSize), (yCenter + ySize), (zCenter - zSize))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter - xSize), (yCenter - ySize), (zCenter + zSize))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter + xSize), (yCenter + ySize), (zCenter - zSize))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter + xSize), (yCenter - ySize), (zCenter + zSize))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter - xSize), (yCenter + ySize), (zCenter + zSize))) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(m_Planes[i], new Vector3((xCenter + xSize), (yCenter + ySize), (zCenter + zSize))) >= 0.0f)
                    continue;

                return false;
            }

            return true;
        }
    }
}
