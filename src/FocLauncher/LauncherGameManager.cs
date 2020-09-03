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
        public event EventHandler<IGame> GameStarted;
        public event EventHandler<IGame> GameClosed;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static LauncherGameManager? _instance;
        private IGame? _eaW;
        private IGame? _foC;

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

        public static LauncherGameManager Instance => _instance ??= new LauncherGameManager();

        private LauncherGameManager()
        {
        }

        public void Initialize(GameDetection gameDetection)
        {
            if (gameDetection is null)
                throw new ArgumentNullException(nameof(gameDetection));
            if (gameDetection.IsError)
                throw new PetroglyphGameException();

            EmpireAtWar = GameFactory.CreateEawGame(gameDetection.EawExe!.Directory, gameDetection.FocType);
            ForcesOfCorruption = GameFactory.CreateFocGame(gameDetection.FocExe!.Directory, gameDetection.FocType);

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

        }

        private void UnRegisterEvent(IGame game)
        {

        }
        
        private void OnGameStarted(IGame e)
        {
            GameStarted?.Invoke(this, e);
        }

        private void OnGameClosed(IGame e)
        {
            GameClosed?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
