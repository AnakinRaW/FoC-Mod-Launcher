using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using FocLauncher.Game;
using FocLauncher.Mods;
using FocLauncher.Properties;
using FocLauncher.Theming;

namespace FocLauncher
{
    public class LauncherDataModel : IDataModel, IDebugPrinter
    {
        public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"FoC Launcher\");
        public static string IconPath = Path.Combine(AppDataPath, "foc.ico");

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Initialized;

        private IGame _eaW;
        private IGame _foC;
        private GameType _focGameType;
        private IEnumerable<IMod> _mods;
        private IMod _selectedMod;
        private bool _useDebugBuild;
        private bool _ignoreAsserts = true;
        private bool _noArtProcess = true;
        private PetroglyphGameManager _gameManager;

        internal static LauncherDataModel Instance { get; private set; }


        public IGame EaW
        {
            get => _eaW;
            set
            {
                if (Equals(value, _eaW))
                    return;
                _eaW = value;
                OnPropertyChanged();
            }
        }

        public PetroglyphGameManager GameManager
        {
            get => _gameManager;
            private set
            {
                if (value == _gameManager)
                    return;
                _gameManager = value;
                OnPropertyChanged();
            }
        }

        public IGame FoC
        {
            get => _foC;
            set
            {
                if (Equals(value, _foC))
                    return;
                _foC = value;
                OnPropertyChanged();
            }
        }

        public GameType FocGameType
        {
            get => _focGameType;
            set
            {
                if (value == _focGameType)
                    return;
                _focGameType = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<IMod> Mods
        {
            get => _mods;
            set
            {
                if (Equals(value, _mods)) return;
                _mods = value;
                OnPropertyChanged();
            }
        }

        public IMod SelectedMod
        {
            get => _selectedMod;
            set
            {
                if (Equals(value, _selectedMod)) return;
                _selectedMod = value;
                OnPropertyChanged();
            }
        }

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

        public void Initialize()
        {
            if (Instance != null)
                throw new InvalidOperationException("The Launcher already was initialized");
            Instance = this;
            InitAppDataDirectory();

            GameManager = PetroglyphGameManager.Instance;
            GameManager.Initialize(AppDataPath); ;

            if (!InitGames(out var result))
            {
                string message = string.Empty;
                if (string.IsNullOrEmpty(result.EawPath))
                    message = "Could not find Empire at War!\r\n";
                else if (string.IsNullOrEmpty(result.FocPath))
                    message += "Could not find Forces of Corruption\r\n";

                MessageBox.Show(message + "\r\nThe launcher will now be closed", "FoC Launcher", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            SearchMods();
            RegisterThemes();
            OnInitialized();
        }

        public string GetDebugInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("----------DEBUG Information----------");
            sb.AppendLine(EaW == null ? "EaW is null" : $"EaW found at: {EaW.GameDirectory};");
            sb.AppendLine(FoC == null ? "FoC is null" : $"FoC found at: {FoC.GameDirectory}; FoC Version: {FocGameType}");
            return sb.ToString();
        }

        private bool InitGames(out GameDetectionResult result)
        {
            result = GameHelper.GetGameInstallations();

            if (result.IsError)
                return false;

            try
            {
                EaW = new Eaw(result.EawPath);
            }
            catch
            {
                return false;
            }

            switch (result.FocType)
            {
                case GameType.SteamGold:
                    FoC = new SteamGame(result.FocPath);
                    SteamModNamePersister.CreateInstance();
                    break;
                case GameType.Disk:
                case GameType.Origin:
                case GameType.GoG:
                case GameType.DiskGold:
                    FoC = new Foc(result.FocPath, result.FocType);
                    break;
            }

            FocGameType = result.FocType;
            return true;
        }

        private void SearchMods()
        {
            Mods = ModHelper.FindMods(FoC);
            SelectedMod = Mods.FirstOrDefault();
        }

        private void RegisterThemes()
        {
            foreach (var mod in Mods)
                RegisterTheme(mod);
        }

        private static void RegisterTheme(IMod mod)
        {
            var custom = mod.ModInfoFile?.Custom;
            if (custom == null || !custom.ContainsKey("launcherTheme"))
                return;
            var relativeThemePath = custom.Value<string>("launcherTheme");
            var themePath = Path.Combine(mod.ModDirectory, relativeThemePath);
            if (!File.Exists(themePath))
                return;

            var theme = ThemeManager.GetThemeFromFile(themePath);
            if (theme == null)
                return;

            ThemeManager.Instance.RegisterTheme(theme);
            ThemeManager.Instance.AssociateThemeToMod(mod, theme);
        }

        private void InitAppDataDirectory()
        {
            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);
            if (File.Exists(IconPath))
                return;
            using (var fs = new FileStream(IconPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                Resources.foc.Save(fs);
            }
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }
    }
}