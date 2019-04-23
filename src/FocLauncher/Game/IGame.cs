using FocLauncher.Mods;

namespace FocLauncher.Game
{
    public interface IGame
    {
        /// <summary>
        /// Returns the full Path of the Games Root Directory
        /// </summary>
        string GameDirectory { get; }

        /// <summary>
        /// Returns the name of the Game
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Contains Data of the Process
        /// </summary>
        GameProcessData GameProcessData { get; }

        /// <summary>
        /// Checks whether a mod exists
        /// </summary>
        /// <returns></returns>
        bool Exists();

        /// <summary>
        /// Plays the default game
        /// </summary>
        void PlayGame();

        /// <summary>
        /// Plays the game with the mod
        /// </summary>
        /// <param name="mod"></param>
        void PlayGame(IMod mod, DebugOptions debugOptions);

        /// <summary>
        /// Checks if the patch is installed
        /// </summary>
        /// <returns></returns>
        bool IsPatched();

        /// <summary>
        /// Determines whether the game AI files are cleared
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if game is clear; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsGameAiClear();
    }
}