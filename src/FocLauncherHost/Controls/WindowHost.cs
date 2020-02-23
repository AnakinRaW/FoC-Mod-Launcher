using System.Windows;
using System.Windows.Controls;

namespace FocLauncherHost.Controls
{
    public class WindowHost : UserControl
    {
        protected readonly Window HostWindow;

        public WindowHost()
        {
            HostWindow = new Window
            {
                Title = "FoC Launcher",
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            };
        }

        public virtual void ShowDialog()
        {
            HostWindow.Content = this;
            HostWindow.ShowDialog();
        }
    }
}
