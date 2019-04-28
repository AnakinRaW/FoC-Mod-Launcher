using System;
using System.Net.NetworkInformation;
using System.Windows.Input;

namespace FocLauncherApp.WaitDialog
{
    public partial class WaitWindowDialog
    {
        public event EventHandler Cancelled;

        public WaitWindowDialog(IntPtr hostMainWindowHandle, int hostProcessId)
        {
            InitializeComponent();
        }

        private void OnTitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
