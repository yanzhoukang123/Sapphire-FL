using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices; // DllImportAttribute

namespace Azure.Utilities
{
    public class WindowsInvoke
    {
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_DLGMODALFRAME = 0x0001;
        public const int WS_MAXIMIZEBOX = 0x10000;

        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_FRAMECHANGED = 0x0020;

        public const int WM_SETICON = 0x0080;

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, int flags);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, int length);
    }
}
