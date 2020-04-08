using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

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

        internal string ApplicationPath { get; private set; }

        // Make this a real instance
        private PetroglyphGameManager()
        {
        }

        // TODO: Consume GameDetection
        public void Initialize(string localAppPath)
        {
            if (!Directory.Exists(localAppPath))
                throw new DirectoryNotFoundException();
            ApplicationPath = localAppPath;
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
