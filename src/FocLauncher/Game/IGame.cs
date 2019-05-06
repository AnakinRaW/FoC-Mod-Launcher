using System;
using System.Diagnostics;
using FocLauncher.Core.Mods;

namespace FocLauncher.Core.Game
{
    public interface IGame
    {
        event EventHandler<Process> GameStarted;

        event EventHandler GameStarting;

        event EventHandler GameClosed;

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
        GameProcessWatcher GameProcessWatcher { get; }

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
        /// <param name="mod">The mod instance that should be started</param>
        /// <param name="args">Additional arguments</param>
        void PlayGame(IMod mod, GameRunArguments args);

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

        /// <summary>
        /// Determines whether the the given language is installed
        /// </summary>
        /// <param name="language">The english name of the language</param>
        /// <returns><see langword="true"/> if game is the language is available; otherwise, <see langword="false"/>.</returns>
        bool IsLanguageInstalled(string language);

        bool HasDebugBuild();
    }
}