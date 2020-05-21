using System;
using System.Collections.Generic;
using System.Diagnostics;
using FocLauncher.Mods;

namespace FocLauncher.Game
{
    public interface IGame : IPetroglyhGameableObject, IModContainer, IHasDirectory
    {
        event EventHandler<Process> GameStarted;

        event EventHandler<GameStartingEventArgs> GameStarting;

        event EventHandler GameClosed;

        GameType Type { get; }
        
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
        bool PlayGame(string? iconFile = null);

        /// <summary>
        /// Plays the game with the mod
        /// </summary>
        /// <param name="args">Arguments which specify more options, including the mod, that shall get used.</param>
        /// <returns><c>true</c> when the game process was invoked; <c>false</c> otherwise. E.g. when the procedure was cancelled.</returns>
        bool PlayGame(GameCommandArguments args, string? iconFile = null);

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

        /// <summary>
        /// Searches for mods for this game on disk and creates a new instance of an <see cref="IMod" when necessary/>
        /// </summary>
        /// <param name="invalidateMods">When set to <c>true</c> the <see cref="IGame.Mods"/> collection
        /// gets updated. This will raise the <see cref="IGame.ModCollectionModified"/> event </param>
        /// <returns>An unsorted collection of all mods found.</returns>
        IReadOnlyCollection<IMod> SearchMods(bool invalidateMods);

        IMod CreateMod(ModCreationDelegate modCreation, bool shallAdd);

        bool TryCreateMod(ModCreationDelegate modCreation, bool shallAdd, out IMod mod);
    }
}