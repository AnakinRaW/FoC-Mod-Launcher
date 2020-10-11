using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using FocLauncher.Game;
using FocLauncher.Game.Detection;
using NLog;

namespace FocLauncher
{
    public sealed class LauncherGameManager : INotifyPropertyChanged
    {
        public event EventHandler<GameStartedEventArgs>? GameStarted;
        public event EventHandler<IGame>? GameClosed;
        public event PropertyChangedEventHandler? PropertyChanged;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static LauncherGameManager? _instance;
        private IGame? _eaW;
        private IGame? _foC;
        private bool _anyGameRunning;
        private readonly GameRunningTracker _tracker;


        public bool AnyGameRunning
        {
            get => _anyGameRunning;
            private set
            {
                if (value == _anyGameRunning)
                    return;
                _anyGameRunning = value;
                OnPropertyChanged();
            }
        }


        public IGame? EmpireAtWar
        {
            get => _eaW;
            private set
            {
                if (_eaW == value)
                    return;
                _eaW = value;
                OnPropertyChanged();
            }
        }

        public IGame? ForcesOfCorruption
        {
            get => _foC;
            private set
            {
                if (_foC == value)
                    return;
                _foC = value;
                OnPropertyChanged();
            }
        }

        public bool Initialized { get; private set; }

        public static LauncherGameManager Instance => _instance ??= new LauncherGameManager();

        private LauncherGameManager()
        {
            _tracker = new GameRunningTracker(this);
        }

        public void Initialize(GameDetection gameDetection)
        {
            if (gameDetection is null)
                throw new ArgumentNullException(nameof(gameDetection));
            if (gameDetection.IsError)
                throw new PetroglyphGameException();

            EmpireAtWar = GameFactory.CreateEawGame(gameDetection.EawExe!.Directory!, gameDetection.EawType);
            ForcesOfCorruption = GameFactory.CreateFocGame(gameDetection.FocExe!.Directory!, gameDetection.FocType);

            Initialized = true;

            LogInstalledGames();

            RegisterEvents(EmpireAtWar);
            RegisterEvents(ForcesOfCorruption);
        }

        private void LogInstalledGames()
        {
            var sb = new StringBuilder();
            sb.AppendLine("----------Installed Game Information----------");
            sb.AppendLine(EmpireAtWar == null ? "EaW is null" : $"EaW found at: {EmpireAtWar.Directory};");
            sb.AppendLine(ForcesOfCorruption == null ? "FoC is null" : $"FoC found at: {ForcesOfCorruption.Directory}; FoC Version: {ForcesOfCorruption.Type}");
            Logger.Info(sb.ToString());
        }

        private void RegisterEvents(IGame game)
        {
            if (!Initialized)
                return;
            EmpireAtWar!.GameStarted += OnGameStarted;
            EmpireAtWar!.GameClosed += OnGameClosed;
            ForcesOfCorruption!.GameStarted += OnGameStarted;
            ForcesOfCorruption!.GameClosed += OnGameClosed;
        }

        private void OnGameStarted(object sender, GameStartedEventArgs e)
        {
            GameStarted?.Invoke(this, e);
            _tracker.GameStarted();
        }

        private void OnGameClosed(object sender, EventArgs e)
        {
            if (!(sender is IGame game))
                return;
            GameClosed?.Invoke(this, game);
            _tracker.GameClosed();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private class GameRunningTracker
        {
            private readonly LauncherGameManager _manager;

            private int _numberOfRunningGames;


            public GameRunningTracker(LauncherGameManager manager)
            {
                _manager = manager;
            }

            public void GameStarted()
            {
                lock (this)
                {
                    if (_numberOfRunningGames == 0)
                        _manager.AnyGameRunning = true;
                    _numberOfRunningGames++;
                }
            }

            public void GameClosed()
            {
                if (_numberOfRunningGames == 0)
                    return;
                _numberOfRunningGames--;
                if (_numberOfRunningGames == 0)
                    _manager.AnyGameRunning = false;
            }
        }
    }
}
