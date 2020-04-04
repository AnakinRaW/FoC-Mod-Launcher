using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace FocLauncherHost.Dialogs
{
    public class WindowHost : UserControl
    {
        protected readonly Window HostWindow;
        private readonly IntPtr _dialogWindowHandle;

        public WindowHost()
        {
            HostWindow = new Window
            {
                Title = "FoC Launcher",
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            };
            _dialogWindowHandle = new WindowInteropHelper(HostWindow).Handle;
        }

        public virtual void ShowDialog()
        {
            SetWindowPos(_dialogWindowHandle, new IntPtr(-1), 0, 0, 0, 0, 19);
            HostWindow.Content = this;
            HostWindow.ShowDialog();
        }

        [DllImport("User32", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);
    }
}
