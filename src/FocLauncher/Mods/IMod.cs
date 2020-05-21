using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FocLauncher.Game;
using FocLauncher.ModInfo;

namespace FocLauncher.Mods
{
    public interface IMod : IPetroglyhGameableObject, IModContainer
    {
        // TODO: This should be in a separate interface
        /// <summary>
        /// Returns the full Path of the Mods Root Directory
        /// </summary>
        //DirectoryInfo? ModDirectory { get; }

        /// <summary>
        /// The <see cref="IGame"/> this mod is associated with.
        /// </summary>
        IGame Game { get; }

        /// <summary>
        /// The <see cref="ModType"/> of this mod.
        /// </summary>
        ModType Type { get; }

        /// <summary>
        /// Identifies whether the mod is a Steam Workshop instance
        /// </summary>
        bool WorkshopMod { get; }

        /// <summary>
        /// Returns <c>true</c> when this mod instance is not physically present; <c>false</c> otherwise
        /// </summary>
        bool Virtual { get; }

        /// <summary>
        /// If a modinfo.json file is available its data gets stored here; otherwise this returns <see langword="null"/>
        /// </summary>
        ModInfoData? ModInfo { get; }

        bool HasDependencies { get; }

        bool DependenciesResolved { get; }

        /// <summary>
        /// Contains the direct dependencies of this mod.
        /// Thus it does not contain the dependencies of a dependency.
        /// </summary>
        IReadOnlyList<IMod> Dependencies { get; }

        /// <summary>
        /// Searches for direct <see cref="IMod"/> dependencies. It does not resolve recursively.
        /// Updates <see cref="Dependencies"/> property. 
        /// </summary>
        /// <returns><c>true</c> if this instance has dependencies; <c>false</c> otherwise</returns>
        bool ResolveDependencies();

        /// <summary>
        /// Converts this mod into a command line argument that can be used for starting the mod.
        /// </summary>
        /// <param name="includeDependencies">When <c>true</c> this methods returns a valid argument that contains the whole dependency chain.</param>
        /// <remarks>This method does not re-resolve dependencies but takes whatever there is in <see cref="Dependencies"/></remarks>
        /// <returns>A valid command line argument.</returns>
        string ToArgs(bool includeDependencies);
    }
}
