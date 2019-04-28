using System.Windows.Input;

namespace FocLauncherApp.WaitDialog
{
    public partial class WaitWindowDialog
    {
        public WaitWindowDialog()
        {
            InitializeComponent();
        }

        private void OnTitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
