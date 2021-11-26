using System.ComponentModel;

namespace FocLauncher.Game
{
    public class GameStartingEventArgs : CancelEventArgs
    {
        public IGame Game { get; }

        public GameCommandArguments GameArguments { get; }

        public GameBuildType BuildType { get; }

        public GameStartingEventArgs(IGame game, GameCommandArguments arguments, GameBuildType buildType)
        {
            Game = game;
            GameArguments = arguments;
            BuildType = buildType;
        }
    }
}