﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using FocLauncher.Core.ModInfo;
using FocLauncher.Core.Properties;
using FocLauncher.Core.Versioning;

namespace FocLauncher.Core.Mods
{
    public class DummyMod : IMod
    {
        public string FolderName => string.Empty;
        public string ModDirectory => string.Empty;
        public string Description => "This is the unmodified version of Forces of Corruption";
        public string Name => "Forces of Corruption (unmodified)";
        public ModVersion Version { get; } = null;
        public bool WorkshopMod { get; } = false;
        public string IconFile { get; } = LauncherDataModel.IconPath;
        public ModInfoFile? ModInfoFile => null;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}