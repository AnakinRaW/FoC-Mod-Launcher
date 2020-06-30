using System;
using FocLauncher.Game.Detection;

namespace FocLauncher.Game
{
    public sealed class LauncherGameManager
    {
        public event EventHandler<IGame> GameStarted;

        public event EventHandler<IGame> GameClosed;

        public IGame EmpireAtWar { get; private set; }

        public IGame ForcesOfCorruption { get; private set; }
        
        public LauncherGameManager(GameDetection gameDetection)
        {
            Initialize(gameDetection);
        }

        private void Initialize(GameDetection gameDetection)
        {
            if (gameDetection is null)
                throw new ArgumentNullException(nameof(gameDetection));
            if (gameDetection.IsError)
                throw new PetroglyphGameException();

            EmpireAtWar = GameFactory.CreateEawGame(gameDetection.EawExe!.Directory, gameDetection.FocType);
            ForcesOfCorruption = GameFactory.CreateFocGame(gameDetection.FocExe!.Directory, gameDetection.FocType);

            RegisterEvents(EmpireAtWar);
            RegisterEvents(ForcesOfCorruption);
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
    }
}
