using System.ComponentModel;
using FocLauncher.Versioning;

namespace FocLauncher.Mods
{
    public interface IMod : INotifyPropertyChanged
    {
        /// <summary>
        /// Returns the Name of the Folder, where the mod is stored
        /// </summary>
        string FolderName { get; }

        /// <summary>
        /// Returns the full Path of the Mods Root Directory
        /// </summary>
        string ModDirectory { get; }

        /// <summary>
        /// Returns the name of the Mod
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Read the Version from a Mod. Sets the Version of a mod
        ///
        /// </summary>
        ModVersion Version { get; }

        /// <summary>
        /// Identifies whether the mod is a Steam Workshop instance
        /// </summary>
        bool WorkshopMod { get; }

        /// <summary>
        /// The path of the icon file of the mod
        /// </summary>
        string IconFile { get; }
    }
}
