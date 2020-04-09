using NLog;

namespace FocLauncher.Game.Detection
{
    public abstract class GameDetector
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public GameDetection DetectGames()
        {
            return DetectGamesCore();
        }

        protected abstract GameDetection DetectGamesCore();
    }
}