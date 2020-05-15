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
    public class MainWindowViewModel : ILauncherWindowModel
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _windowed;
        private GameType _gameType;
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
                if (_session == null){
                    // TODO: Remove game from ctor
                    _session = new LauncherSession(this, _foC);
                    _session.Started += OnGameStarted;
                    _session.StartFailed += OnGameStartFailed;
                }
                return _session;
            }
        }
        
        public ICommand LaunchCommand => new UICommand(ExecutedLaunch, CanExecute);

        public ObservableCollection<LauncherListItemModel> GameObjects { get; } = new ObservableCollection<LauncherListItemModel>();

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
            }
        }

        public IGame EaW
        {
            get => _eaW;
            set
            {
                if (_eaW == value)
                    _eaW = value;
                OnPropertyChanged();
            }
        }

        public GameType GameType
        {
            get => _gameType;
            set
            {
                if (value == _gameType)
                    return;
                _gameType = value;
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
        
        public MainWindowViewModel()
        {
            InitializeAsync().Forget();
        }

        private async Task InitializeAsync()
        {
            await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                var gameDetection = await FindGamesAsync();
                LogInstalledGames();
                var gameManager = new PetroglyphGameManager(gameDetection);
                
                GameManager = gameManager;
                FoC = gameManager.ForcesOfCorruption;
                EaW = gameManager.EmpireAtWar;


                GameObjects.Add(new LauncherListItemModel(FoC, LauncherSession));


                //foreach (var gameObject in initialization.SearchGameObjects())
                //    GameObjects.Add(new LauncherListItemModel(gameObject, LauncherSession));

                return Task.CompletedTask;
            });


            _initialized = true;
            CommandManager.InvalidateRequerySuggested();
        }

        private static async Task<GameDetection> FindGamesAsync()
        {
            await TaskScheduler.Default;
            var gameDetection = GameDetectionHelper.GetGameInstallations();
            if (gameDetection.IsError)
            {
                var message = string.Empty;
                if (gameDetection.EawExe == null || !gameDetection.EawExe.Exists)
                    message = "Could not find Empire at War!\r\n";
                else if (gameDetection.FocExe == null || !gameDetection.FocExe.Exists)
                    message += "Could not find Forces of Corruption\r\n";

                MessageBox.Show(message + "\r\nThe launcher will now be closed", "FoC Launcher", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(true);
                Application.Current.Shutdown();
            }

            return gameDetection;
        }

        internal IReadOnlyCollection<IPetroglyhGameableObject> SearchGameObjects()
        {
            if (_gameObjects == null)
            {
                _gameObjects = new HashSet<IPetroglyhGameableObject> { FoC };
                foreach (var mod in ModHelper.FindMods(FoC))
                    _gameObjects.Add(mod);
            }
            return _gameObjects.ToList();
        }

        private void LogInstalledGames()
        {
            var sb = new StringBuilder();
            sb.AppendLine("----------Installed Game Information----------");
            sb.AppendLine(EaW == null ? "EaW is null" : $"EaW found at: {EaW.GameDirectory};");
            sb.AppendLine(FoC == null ? "FoC is null" : $"FoC found at: {FoC.GameDirectory}; FoC Version: {FoC.Type}");
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
                _session.Invoke(new[] { gameable });
        }

        private bool CanExecute(object obj)
        {
            return _initialized && obj is IPetroglyhGameableObject;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
