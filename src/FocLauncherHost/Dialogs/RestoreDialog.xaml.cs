using System.Windows;

namespace FocLauncherHost.Dialogs
{
    public partial class RestoreDialog
    {
        public bool Restore { get; private set; }

        public RestoreDialog()
        {
            InitializeComponent();
        }

        private void OnRestore(object sender, RoutedEventArgs e)
        {
            Restore = true;
        }
    }
}
