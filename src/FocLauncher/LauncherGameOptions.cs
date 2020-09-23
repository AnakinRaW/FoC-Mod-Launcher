using System.ComponentModel;
using System.Runtime.CompilerServices;
using FocLauncher.Game;

namespace FocLauncher
{
    internal class LauncherGameOptions : INotifyPropertyChanged
    {
        private static LauncherGameOptions? _instance;
        private bool _windowed;
        private bool _noArtProcess = true;
        private bool _ignoreAsserts;
        private bool _useDebugBuild;
        private string _language;
        public event PropertyChangedEventHandler PropertyChanged;

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

        public string Language
        {
            get => _language;
            set
            {
                if (value == _language) return;
                _language = value;
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
            args.Langauge = Language;
        }
    }
}