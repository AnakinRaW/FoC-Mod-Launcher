using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FocLauncherHost.Controls
{
    public partial class RestartElevatedWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool ElevateResult { get; private set; }

        public RestartElevatedWindow()
        {
            InitializeComponent();
        }

        public override void ShowDialog()
        {
            HostWindow.Topmost = true;
            HostApplication.SplashVisibleResetEvent.WaitOne(TimeSpan.FromSeconds(2));
            if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
                HostWindow.Owner = Application.Current.MainWindow;
            base.ShowDialog();
        }

        private void OnRestartClick(object sender, RoutedEventArgs e)
        {
            ElevateResult = true;
            HostWindow.Close();
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
