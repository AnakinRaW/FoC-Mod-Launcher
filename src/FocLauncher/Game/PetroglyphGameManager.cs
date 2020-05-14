using System;
using System.Threading.Tasks;
using FocLauncher.Game.Detection;

namespace FocLauncher.Game
{
    public sealed class PetroglyphGameManager
    {
        public event EventHandler<IGame> GameStarted;

        public event EventHandler<IGame> GameClosed;

        public event EventHandler Initialized;
        
        public IGame EmpireAtWar { get; private set; }

        public IGame ForcesOfCorruption { get; private set; }
        

        // TODO: Make this a real instance
        public PetroglyphGameManager(GameDetection gameDetection)
        {
            Initialize(gameDetection);
        }

        private void Initialize(GameDetection gameDetection)
        {
            if (gameDetection.IsError)
                throw new PetroglyphGameException();

            EmpireAtWar = GameFactory.CreateEawGame(gameDetection.EawExe.Directory, gameDetection.FocType);
            ForcesOfCorruption = GameFactory.CreateFocGame(gameDetection.FocExe.Directory, gameDetection.FocType);
        }

        // TODO
        //public void FindAndInitialize(GameDetectionOptions findOptions)
        //{
        //}

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

        private void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }
    }
}
