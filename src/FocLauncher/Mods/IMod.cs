using System.ComponentModel;
using FocLauncher.Game;
using FocLauncher.ModInfo;

namespace FocLauncher.Mods
{
    public interface IMod : IPetroglyhGameableObject, INotifyPropertyChanged
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
        /// Identifies whether the mod is a Steam Workshop instance
        /// </summary>
        bool WorkshopMod { get; }

        /// <summary>
        /// If a modinfo.json file is available its data gets stored here; otherwise this returns <see langword="null"/>
        /// </summary>
        ModInfoFile? ModInfoFile { get; }
    }
}
