using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FocLauncher.Game.Detection;

namespace FocLauncher.Game
{
    public sealed class PetroglyphGameManager : INotifyPropertyChanged
    {
        public event EventHandler<IGame> GameStarted;

        public event EventHandler<IGame> GameClosed;

        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly Lazy<PetroglyphGameManager> Lazy =
            new Lazy<PetroglyphGameManager>(() => new PetroglyphGameManager());

        public static PetroglyphGameManager Instance => Lazy.Value;

        public IGame EmpireAtWar { get; private set; }

        public IGame ForcesOfCorruption { get; private set; }
        

        // TODO: Make this a real instance
        private PetroglyphGameManager()
        {
        }

        // TODO: Consume GameDetection
        public void Initialize(GameDetection gameDetection)
        {

        }

        public void FindAndInitialize(GameDetectionOptions findOptions)
        {

        }

        private void RegisterEvents(IGame game)
        {

        }

        private void UnRegisterEvent(IGame game)
        {

        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
