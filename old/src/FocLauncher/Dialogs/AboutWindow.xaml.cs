using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Navigation;
using FocLauncher.Theming;

namespace FocLauncher.Dialogs
{
    public partial class AboutWindow : INotifyPropertyChanged
    {
        private Version _launcherVersion;
        private Version _themeVersion;

        public Version LauncherVersion
        {
            get => _launcherVersion;
            set
            {
                if (Equals(value, _launcherVersion)) return;
                _launcherVersion = value;
                OnPropertyChanged();
            }
        }

        public Version ThemeVersion
        {
            get => _themeVersion;
            set
            {
                if (Equals(value, _themeVersion)) return;
                _themeVersion = value;
                OnPropertyChanged();
            }
        }

        public AboutWindow(Window owner) : this()
        {
            Owner = owner;
        }

        public AboutWindow()
        {
            InitializeComponent();
            _launcherVersion = GetType().Assembly.GetName().Version;
            _themeVersion = typeof(ITheme).Assembly.GetName().Version;
        }

        private void OpenLicenseSite(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
