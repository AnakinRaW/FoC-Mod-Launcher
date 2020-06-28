using System;

namespace FocLauncher.Game
{
    public static class GameRunHelper
    {
        public static bool RunGame(IGame game, GameCommandArguments? arguments, string? iconFile = null, bool debug = false)
        {
            if (game is null)
                throw new ArgumentNullException(nameof(game));
            if (debug && game is IDebugable debugable)
                return debugable.DebugGame(arguments, iconFile);
            return game.PlayGame(arguments, iconFile);
        }
    }
}