using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FocLauncher.Game;
using FocLauncher.Game.Detection;
using FocLauncher.Input;
using FocLauncher.Mods;
using FocLauncher.Properties;
using FocLauncher.Theming;
using FocLauncher.Threading;
using Microsoft.VisualStudio.Threading;
using NLog;

namespace FocLauncher
{
    /*
    public class PetroglyphInitialization
    {
        private ICollection<IPetroglyhGameableObject> _gameObjects;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Initialized;

        public void Initialize()
        {
            SteamModNamePersister.CreateInstance();

            SearchGameObjects();
            RegisterThemes();
            OnInitialized();
        }

        private void RegisterThemes()
        {
            foreach (var mod in _gameObjects.OfType<IMod>())
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
    }
    */

    public class MainWindowViewModel : ILauncherWindowModel
    {
        private readonly MainWindow _window;
        internal event EventHandler<GameDetection> GamesDetected;

        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _windowed;
        private bool _useDebugBuild;
        private bool _ignoreAsserts = true;
        private bool _noArtProcess = true;
        private LauncherSession _session;
        private IGame _foC;
        private IGame _eaW;

        private bool _initialized;

        internal LauncherSession LauncherSession
        {
            get
            {
                if (_session == null)
                {
                    // TODO: Remove game from ctor
                    _session = new LauncherSession(this, _foC);
                    _session.Started += OnGameStarted;
                    _session.StartFailed += OnGameStartFailed;
                }

                return _session;
            }
        }

        public ICommand LaunchCommand => new UICommand(ExecutedLaunch, CanExecute);

        public ObservableCollection<LauncherListItemModel> GameObjects { get; } =
            new ObservableCollection<LauncherListItemModel>();

        public IReadOnlyCollection<IMod> Mods => GameObjects.Select(x => x.GameObject).OfType<IMod>().ToList();

        public PetroglyphGameManager GameManager { get; private set; }

        public IGame FoC
        {
            get => _foC;
            set
            {
                if (_foC == value)
                    return;
                _foC = value;
                OnPropertyChanged();
                FocType = _foC == null ? GameType.Undefined : value.Type;
                OnPropertyChanged(nameof(FocType));
            }
        }

        public IGame EaW
        {
            get => _eaW;
            set
            {
                if (_eaW == value)
                    return;
                _eaW = value;
                OnPropertyChanged();
                EaWType = _eaW == null ? GameType.Undefined : value.Type;
                OnPropertyChanged(nameof(EaWType));
            }
        }

        public GameType FocType { get; private set; }

        public GameType EaWType { get; private set; }

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

        public MainWindowViewModel(MainWindow window)
        {
            _window = window;
            GamesDetected += OnGameDetectionFinished;
            // TODO: Add additional state so that FocType=undefined and not initialized will state this in UI! 
            FindGamesAsync().ForgetButThrow();
        }

        private void OnGameDetectionFinished(object sender, GameDetection e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var gameManager = new PetroglyphGameManager(e);

            GameManager = gameManager;
            FoC = gameManager.ForcesOfCorruption;
            EaW = gameManager.EmpireAtWar;
            LogInstalledGames();

            FoC.Setup(GameSetupOptions.ResolveModDependencies);
            
            GameObjects.Add(new LauncherListItemModel(FoC, LauncherSession));
            foreach (var gameObject in FoC.Mods)
                GameObjects.Add(new LauncherListItemModel(gameObject, LauncherSession));
            _window.ListBox.FocusSelectedItem();
            _initialized = true;
        }

        private async Task FindGamesAsync()
        {
            await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                var shuttingDown = false;
                try
                {
                    await TaskScheduler.Default;
                    var gameDetection = GameDetection.NotInstalled;
                    await Task.Run(async () =>
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        gameDetection = GameDetectionHelper.GetGameInstallations();
                    }).ConfigureAwait(false);

                    if (gameDetection.IsError)
                    {
                        shuttingDown = true;
                        var message = string.Empty;
                        if (gameDetection.EawExe == null || !gameDetection.EawExe.Exists)
                            message = "Could not find Empire at War!\r\n";
                        else if (gameDetection.FocExe == null || !gameDetection.FocExe.Exists)
                            message += "Could not find Forces of Corruption\r\n";
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        MessageBox.Show(message + "\r\nThe launcher will now be closed", "FoC Launcher",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        Application.Current.Shutdown();
                    }

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    OnGamesDetected(gameDetection);
                }
                catch (Exception e)
                {
                    if (shuttingDown)
                        return;
                    Logger.Error(e, $"Failed to initialize MainWindow view model: {e.Message}");
                    throw;
                }
            });
        }
        
        private void LogInstalledGames()
        {
            var sb = new StringBuilder();
            sb.AppendLine("----------Installed Game Information----------");
            sb.AppendLine(EaW == null ? "EaW is null" : $"EaW found at: {EaW.Directory};");
            sb.AppendLine(FoC == null ? "FoC is null" : $"FoC found at: {FoC.Directory}; FoC Version: {FoC.Type}");
            Logger.Info(sb.ToString());
        }

        private static void OnGameStartFailed(object sender, IPetroglyhGameableObject e)
        {
            MessageBox.Show($"Unable to start {e.Name}", "FoC Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void OnGameStarted(object sender, IPetroglyhGameableObject e)
        {
            if (Settings.Default.AutoSwitchTheme &&
                ThemeManager.Instance.TryGetThemeByMod(e as IMod, out var theme))
                ThemeManager.Instance.Theme = theme;
        }

        private void ExecutedLaunch(object obj)
        {
            if (obj is IPetroglyhGameableObject gameable)
                _session.Invoke(new[] {gameable});
        }

        private bool CanExecute(object obj)
        {
            return _initialized && obj is IPetroglyhGameableObject;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnGamesDetected(GameDetection e)
        {
            GamesDetected?.Invoke(this, e);
        }
    }
}
