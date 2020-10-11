using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using FocLauncher.Settings;

namespace FocLauncher.Dialogs
{ 
    public partial class SettingsWindow : INotifyPropertyChanged
    {
        private LanguageFallback _languageFallbackOption;
        public FocLauncherInformation Launcher { get; }

        public Dictionary<FallbackSuppression, string> ExampleEnumsWithCaptions { get; } =
            new Dictionary<FallbackSuppression, string>
            {
                {FallbackSuppression.Always, "Do nothing"},
                {FallbackSuppression.Never, "Update last stable version"},
                {FallbackSuppression.Ask, "Show ask dialog"},
            };

        public IEnumerable<LanguageFallbackData> LanguageFallbackItems { get; } 
            = new List<LanguageFallbackData>(new []
            {
                new LanguageFallbackData(LanguageFallback.NoText, "No Text", 
                    "Launcher will fallback to English, if the game/mod has no text translations for the current system language available."),
                new LanguageFallbackData(LanguageFallback.NoFullLocalization, "Not fully localized",
                    "Launcher will fallback to English if the game/mod is not fully localized for the current system language.")
            });


        public LanguageFallback LanguageFallbackOption
        {
            get => _languageFallbackOption;
            set
            {
                if (value.Equals(_languageFallbackOption))
                    return;
                _languageFallbackOption = value;
                Properties.Settings.Default.LanguageFallback = value;
                OnPropertyChanged();
            }
        }

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

        public SettingsWindow(Window owner) : this()
        {
            Owner = owner;
        }

        public SettingsWindow()
        {
            Launcher = FocLauncherInformation.Instance;
            _languageFallbackOption = Properties.Settings.Default.LanguageFallback;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
