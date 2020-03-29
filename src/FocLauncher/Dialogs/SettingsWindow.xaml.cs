using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FocLauncher.Dialogs
{ 
    public partial class SettingsWindow : INotifyPropertyChanged
    {
        public FocLauncherInformation Launcher { get; }

        public Dictionary<FallbackSuppression, string> ExampleEnumsWithCaptions { get; } =
            new Dictionary<FallbackSuppression, string>
            {
                {FallbackSuppression.Always, "Do nothing"},
                {FallbackSuppression.Never, "Update last stable version"},
                {FallbackSuppression.Ask, "Show ask dialog"},
            };


       
        public FallbackSuppression UpdateFallbackOption
        {
            get => Launcher.UpdateFallbackSuppression;
            set
            {
                if (value == UpdateFallbackOption)
                    return;
                Launcher.UpdateFallbackSuppression = value;
                OnPropertyChanged();
            }
        }

        public bool UseBetaBuilds
        {
            get => Launcher.UpdateSearchOption == ApplicationType.Beta;
            set
            {
                if (value == UseBetaBuilds)
                    return;
                Launcher.UpdateSearchOption = !value ? ApplicationType.Stable : ApplicationType.Beta;
                OnPropertyChanged();
            }
        }

        public SettingsWindow()
        {
            Launcher = FocLauncherInformation.Instance;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
