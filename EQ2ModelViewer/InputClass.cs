using System;
using System.Threading.Tasks;
using System.Windows.Forms;

using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DirectInput;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace EQ2ModelViewer
{
    public class InputClass
    {
        private DirectInput m_DirectInput;
        private Keyboard m_Keyboard;
        private KeyboardState m_KeyboardState;
        private Mouse m_mouse;
        private MouseState m_mouseState;

        public bool Initialize(Control control)
        {
            m_DirectInput = new DirectInput();
            m_Keyboard = new Keyboard(m_DirectInput);
            m_mouse = new Mouse(m_DirectInput);
            try
            {
                Result result = m_Keyboard.SetCooperativeLevel(control, CooperativeLevel.Nonexclusive | CooperativeLevel.Background);
                Result result2 = m_mouse.SetCooperativeLevel(control, CooperativeLevel.Nonexclusive | CooperativeLevel.Background);
            }
            catch (DirectInputException e)
            {
                MessageBox.Show(e.Message);
            }
            m_Keyboard.Acquire();
            m_mouse.Acquire();

            return true;
        }

        public void ShutDown()
        {
            m_mouse.Dispose();
            m_Keyboard.Dispose();
            m_DirectInput.Dispose();
        }

        public bool Frame()
        {
            if (!ReadKeyboard())
            {
                Console.WriteLine("InputClass: Failed to read the keyboard.");
                return false;
            }
            if (!ReadMouse()) {
                Console.WriteLine("InputClass: Failed to read the mouse.");
                return false;
            }

            ProcessInput();
            return true;
        }

        private bool ReadKeyboard()
        {
            m_KeyboardState = m_Keyboard.GetCurrentState();
            return true;
        }

        private bool ReadMouse() {
            m_mouseState = m_mouse.GetCurrentState();
            return true;
        }

        private bool ProcessInput()
        {
            return true;
        }

        public bool IsAPressed()
        {
            return m_KeyboardState.IsPressed(Key.A);
        }

        public bool IsZPressed()
        {
            return m_KeyboardState.IsPressed(Key.Z);
        }

        public bool IsPgUpPressed()
        {
            return m_KeyboardState.IsPressed(Key.PageUp);
        }

        public bool IsPgDownPressed()
        {
            return m_KeyboardState.IsPressed(Key.PageDown);
        }

        public bool IsDownPressed()
        {
            return m_KeyboardState.IsPressed(Key.DownArrow);
        }

        public bool IsUpPressed()
        {
            return m_KeyboardState.IsPressed(Key.UpArrow);
        }

        public bool IsLeftPressed()
        {
            return m_KeyboardState.IsPressed(Key.LeftArrow);
        }

        public bool IsRightPressed()
        {
            return m_KeyboardState.IsPressed(Key.RightArrow);
        }

        public bool IsLeftMousePressed() {
            return m_mouseState.IsPressed((int)MouseObject.Button1);
        }

        public bool IsEscapePressed()
        {
            return m_KeyboardState.IsPressed(Key.Escape);
        }

        public bool IsKeyPressed(Key key)
        {
            return m_KeyboardState.IsPressed(key);
        }
        
        public bool IsKeyReleased(Key key)
        {
            return m_KeyboardState.IsReleased(key);
        }
        public bool IsDeletePressed()
        {
            return m_KeyboardState.IsPressed(Key.Delete);
        }

        public int GetMouseX() {

            return RenderForm.MousePosition.X;
        }

        public int GetMouseY() {
            return RenderForm.MousePosition.Y;
        }
    }
}
