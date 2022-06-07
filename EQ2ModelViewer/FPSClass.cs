using System;

namespace EQ2ModelViewer
{
    public class FPSClass
    {
        private int m_FPS;
        private int m_Count;
        private long m_StartTime;

        public void Initialize()
        {
            m_FPS = 0;
            m_Count = 0;
            m_StartTime = Environment.TickCount;
        }

        public void Frame()
        {
            m_Count++;

            if (Environment.TickCount >= m_StartTime + 1000)
            {
                m_FPS = m_Count;
                m_Count = 0;
                m_StartTime = Environment.TickCount;
            }
        }

        public int GetFPS()
        {
            return m_FPS;
        }
    }
}
