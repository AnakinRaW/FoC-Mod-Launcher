using System.ComponentModel;
using FocLauncher.Core.ModInfo;
using FocLauncher.Core.Versioning;

namespace FocLauncher.Core.Mods
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
        /// Returns the description text of the mod
        /// </summary>
        string Description { get; }

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

        /// <summary>
        /// If a modinfo.json file is available its data gets stored here; otherwise this returns <see langword="null"/>
        /// </summary>
        ModInfoFile? ModInfoFile { get; }
    }
}
