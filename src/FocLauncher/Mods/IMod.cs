using System;
using System.Collections.Generic;
using FocLauncher.Game;
using FocLauncher.ModInfo;

namespace FocLauncher.Mods
{
    public interface IMod : IModIdentity, IModReference, IPetroglyhGameableObject, IModContainer, IEquatable<IMod>
    {
        /// <summary>
        /// The <see cref="IGame"/> this mod is associated with.
        /// </summary>
        IGame Game { get; }
        
        ///// <summary>
        ///// The <see cref="ModType"/> of this mod.
        ///// </summary>
        //ModType Type { get; }

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

        new IReadOnlyList<IMod> Dependencies { get; }

        bool HasDependencies { get; }

        bool DependenciesResolved { get; }

        int ExpectedDependencies { get; }

        ///// <summary>
        ///// Contains the direct dependencies of this mod.
        ///// Thus it does not contain the dependencies of a dependency.
        ///// </summary>
        //IReadOnlyList<IModReference> Dependencies { get; }

        /// <summary>
        /// Searches for direct <see cref="IMod"/> dependencies. It does not resolve recursively.
        /// Updates <see cref="Dependencies"/> property.
        /// <param name="resolveStrategy">Option how resolving should be handled.</param>
        /// </summary>
        /// <returns><c>true</c> if all direct dependencies could be resolved; <c>false</c> otherwise</returns>
        /// <exception cref="PetroglyphModException">Throws exception if a dependency cycle was found.</exception>
        bool ResolveDependencies(ModDependencyResolveStrategy resolveStrategy);

        /// <summary>
        /// Converts this mod into a command line argument that can be used for starting the mod.
        /// </summary>
        /// <param name="includeDependencies">When <c>true</c> this methods returns a valid argument that contains the whole dependency chain.</param>
        /// <remarks>This method does not re-resolve dependencies but takes whatever there is in <see cref="Dependencies"/></remarks>
        /// <returns>A valid command line argument.</returns>
        string ToArgs(bool includeDependencies);
    }

    public class ModEqualityComparer : IEqualityComparer<IMod>
    {
        public static readonly ModEqualityComparer Default = new ModEqualityComparer();
        //public static readonly ModEqualityComparer NamEqualityComparer = new ModEqualityComparer(true);

        private readonly bool _default;

        public ModEqualityComparer()
        {
            _default = true;
        }

        public bool Equals(IMod x, IMod y)
        {
            if (x is null || y is null)
                return false;
            if (x == y)
                return true;
            if (_default)
                return x.Equals(y);
            throw new NotImplementedException();
        }

        public int GetHashCode(IMod obj)
        {
            if (_default)
                return obj.GetHashCode();
            throw new NotImplementedException();
        }
    }

    public enum ModDependencyResolveStrategy
    {
        FromExistingMods,
        FromExistingModsRecursive,
        Create,
        CreateRecursive
    }

    internal static class ModDependencyUtilities
    {
        internal static bool IsRecursive(this ModDependencyResolveStrategy strategy)
        {
            return strategy == ModDependencyResolveStrategy.CreateRecursive ||
                   strategy == ModDependencyResolveStrategy.FromExistingModsRecursive;
        }

        internal static bool IsCreative(this ModDependencyResolveStrategy strategy)
        {
            return strategy == ModDependencyResolveStrategy.Create ||
                   strategy == ModDependencyResolveStrategy.CreateRecursive;
        }
    }
}
