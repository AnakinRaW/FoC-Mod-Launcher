using System.ComponentModel;

namespace FocLauncher.Game
{
    public class GameStartingEventArgs : CancelEventArgs
    {
        public GameCommandArguments GameArguments { get; }

        public GameBuildType BuildType { get; }

        public GameStartingEventArgs(GameCommandArguments arguments, GameBuildType buildType)
        {
            GameArguments = arguments;
            BuildType = buildType;
        }
    }
}