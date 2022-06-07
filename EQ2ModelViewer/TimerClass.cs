using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EQ2ModelViewer
{
    public class TimerClass
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceCounter(out long lpFrequency);

        private long m_Frequency = 0;
        private long m_StartTime = 0;
        private float m_TicksPerMS = 0;
        private float m_FrameTime = 0;

        public bool Initialize()
        {
            if (!QueryPerformanceFrequency(out m_Frequency))
            {
                Console.WriteLine("TimerClass: Initialize failed.");
                return false;
            }

            m_TicksPerMS = (float)(m_Frequency / 1000);
            QueryPerformanceCounter(out m_StartTime);
            return true;
        }

        public void Frame()
        {
            long CurrentTime;
            float TimeDif;

            QueryPerformanceCounter(out CurrentTime);
            TimeDif = (float)(CurrentTime - m_StartTime);
            m_FrameTime = TimeDif / m_TicksPerMS;
            m_StartTime = CurrentTime;
        }

        public float GetTime()
        {
            return m_FrameTime;
        }
    }
}
