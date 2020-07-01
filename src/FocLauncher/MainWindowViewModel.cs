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
using FocLauncher.Items;
using FocLauncher.Mods;
using FocLauncher.Properties;
using FocLauncher.Theming;
using FocLauncher.Threading;
using Microsoft.VisualStudio.Threading;
using NLog;

namespace FocLauncher
{
    
    //public class PetroglyphInitialization
    //{
    //    public void Initialize()
    //    {
    //        RegisterThemes();
    //    }

    //    private void RegisterThemes()
    //    {
    //        foreach (var mod in _gameObjects.OfType<IMod>())
    //            RegisterTheme(mod);
    //    }

    //    private static void RegisterTheme(IMod mod)
    //    {
    //        var custom = mod.ModInfo?.Custom;
    //        if (custom == null || !custom.ContainsKey("launcherTheme"))
    //            return;
    //        var relativeThemePath = custom.Value<string>("launcherTheme");
    //        var themePath = Path.Combine(mod.ModDirectory, relativeThemePath);
    //        if (!File.Exists(themePath))
    //            return;

    //        var theme = ThemeManager.GetThemeFromFile(themePath);
    //        if (theme == null)
    //            return;

    //        ThemeManager.Instance.RegisterTheme(theme);
    //        ThemeManager.Instance.AssociateThemeToMod(mod, theme);
    //    }
    //}
    

    public class MainWindowViewModel
    {
        private readonly MainWindow _window;
        internal event EventHandler<GameDetection> GamesDetected;

        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IGame _foC;
        private IGame _eaW;

        private bool _initialized;

        public ICommand LaunchCommand => new UICommand(ExecutedLaunch, CanExecute);

        public ObservableCollection<LauncherItem> GameObjects { get; } =
            new ObservableCollection<LauncherItem>();

        public IReadOnlyCollection<IMod> Mods => GameObjects.Select(x => x.GameObject).OfType<IMod>().ToList();

        public LauncherItemManager ItemManager { get; }

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
            //var gameManager = new LauncherGameManager(e);
            //FoC = gameManager.ForcesOfCorruption;
            //EaW = gameManager.EmpireAtWar;
            LogInstalledGames();

            //FoC.Setup(GameSetupOptions.ResolveModDependencies);

            RegisterEvents();
            
            _initialized = true;
        }

        private void RegisterEvents()
        {
            //FoC.GameStarted += OnGameStarted;
            //EaW.GameStarted += OnGameStarted;
        }

        private static void OnGameStarted(object sender, System.Diagnostics.Process e)
        {
            if (Settings.Default.AutoSwitchTheme &&
                ThemeManager.Instance.TryGetThemeByMod(e as IMod, out var theme))
                ThemeManager.Instance.Theme = theme;
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

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    if (gameDetection.IsError)
                    {
                        shuttingDown = true;
                        CloseApplication(gameDetection);
                        return;
                    }
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
        
        private static void ExecutedLaunch(object obj)
        {
            if (!(obj is IPetroglyhGameableObject gameObject))
                return;
            LauncherGameObjectCommandHandler.Launch(gameObject);
        }

        private bool CanExecute(object obj)
        {
            return _initialized && obj is IPetroglyhGameableObject;
        }

        private static void CloseApplication(GameDetection gameDetection)
        {
            var message = string.Empty;
            if (gameDetection.EawExe == null || !gameDetection.EawExe.Exists)
                message = "Could not find Empire at War!\r\n";
            else if (gameDetection.FocExe == null || !gameDetection.FocExe.Exists)
                message += "Could not find Forces of Corruption\r\n";
            MessageBox.Show(message + "\r\nThe launcher will now be closed", "FoC Launcher",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Application.Current.Shutdown();
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
