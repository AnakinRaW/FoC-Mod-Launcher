namespace FocLauncher.Game
{
    public interface IDebugable
    {
        bool DebugBuildExists { get; }

        // TODO: Add default bool option that allows to throw if no debug build found.
        /// <summary>
        /// Plays the game with the mod
        /// </summary>
        /// <param name="args">Arguments which specify more options, including the mod, that shall get used.</param>
        /// <returns><c>true</c> when the game process was invoked; <c>false</c> otherwise. E.g. when the procedure was cancelled.</returns>
        bool DebugGame(GameCommandArguments? args = null, string? iconFile = null);
    }
}