using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using EawModinfo.Spec;
using FocLauncher.Game;
using FocLauncher.Settings;
using FocLauncher.Utilities;

namespace FocLauncher
{
    internal class LauncherGameOptions : INotifyPropertyChanged
    {
        private static LauncherGameOptions? _instance;
        private bool _windowed;
        private bool _noArtProcess = true;
        private bool _ignoreAsserts;
        private bool _useDebugBuild;
        private bool _fallbackToEnglish = true;
        private bool _useSystemLanguage = true;
        private string _customLanguage;
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool UseDebugBuild
        {
            get => _useDebugBuild;
            set
            {
                if (value == _useDebugBuild) return;
                _useDebugBuild = value;
                OnPropertyChanged();
            }
        }
        
        public bool IgnoreAsserts
        {
            get => _ignoreAsserts;
            set
            {
                if (value == _ignoreAsserts)
                    return;
                _ignoreAsserts = value;
                OnPropertyChanged();
            }
        }

        public bool NoArtProcess
        {
            get => _noArtProcess;
            set
            {
                if (value == _noArtProcess) return;
                _noArtProcess = value;
                OnPropertyChanged();
            }
        }

        public bool Windowed
        {
            get => _windowed;
            set
            {
                if (value == _windowed)
                    return;
                _windowed = value;
                OnPropertyChanged();
            }
        }

        public bool UseSystemLanguage
        {
            get => _useSystemLanguage;
            set
            {
                if (value == _useSystemLanguage)
                    return;
                _useSystemLanguage = value;
                OnPropertyChanged();
            }
        }

        public bool FallbackToEnglish
        {
            get => _fallbackToEnglish;
            set
            {
                if (value == _fallbackToEnglish)
                    return;
                _fallbackToEnglish = value;
                OnPropertyChanged();
            }
        }

        public string CustomLanguage
        {
            get => _customLanguage;
            set
            {
                if (_customLanguage == value)
                    return;
                _customLanguage = value;
                OnPropertyChanged();
            }
        }


        public static LauncherGameOptions Instance => _instance ??= new LauncherGameOptions();

        private LauncherGameOptions()
        {
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void FillArgs(GameCommandArguments args)
        {
            args.IgnoreAsserts = IgnoreAsserts;
            args.NoArtProcess = NoArtProcess;
            args.Windowed = Windowed;
        }

        public string GetLanguageFromOptions(IPetroglyhGameableObject gameObject)
        {
            if (!UseSystemLanguage)
                return CustomLanguage.ToUpperInvariant();

            var systemCulture = CultureInfo.InstalledUICulture;

            if (UseSystemLanguage && !FallbackToEnglish)
                return systemCulture.EnglishName.ToUpperInvariant();

            const string englishName = "ENGLISH";

            var supportedLanguage =
                gameObject.InstalledLanguages.FirstOrDefault(x =>
                    x.Code.Equals(systemCulture.TwoLetterISOLanguageName));

            if (supportedLanguage == null)
                return englishName;

            var supportLevel = supportedLanguage.Support;
            if (Properties.Settings.Default.LanguageFallback == LanguageFallback.NoFullLocalization &&
                !supportLevel.HasFlag(LanguageSupportLevel.FullLocalized))
                return englishName;
            if (Properties.Settings.Default.LanguageFallback == LanguageFallback.NoText &&
                !supportLevel.HasFlag(LanguageSupportLevel.Text))
                return englishName;
            return supportedLanguage.GetLanguageEnglishName();
        }
    }
}