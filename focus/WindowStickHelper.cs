using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace focus
{
    class WindowStickHelper
    {
        const int WM_MOVING = 0x0216;

        int InitialHeight = -1;

        private readonly Window window;

        public WindowStickHelper(Window window)
        {
            this.window = window;
        }

        public void StickWindowToTopOnMove()
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            HwndSource.FromHwnd(helper.Handle).AddHook(HwndMessageHook);
        }

        private IntPtr HwndMessageHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool bHandled)
        {
            switch (msg)
            {
                case WM_MOVING:
                    {
                        WIN32Rectangle rectangle = (WIN32Rectangle)Marshal.PtrToStructure(lParam, typeof(WIN32Rectangle));

                        if (InitialHeight == -1)
                        {
                            InitialHeight = rectangle.Bottom - rectangle.Top;
                        }

                        rectangle.Top = 0;
                        rectangle.Bottom = InitialHeight;

                        bHandled = true;

                        Marshal.StructureToPtr(rectangle, lParam, true);
                    }
                    break;

            }
            return IntPtr.Zero;
        }
    }
}
