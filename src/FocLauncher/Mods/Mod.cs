using System;
using System.IO;
using System.Linq;
using FocLauncher.Game;
using FocLauncher.ModInfo;

namespace FocLauncher.Mods
{
    public class Mod : ModBase, IHasDirectory
    {
        public DirectoryInfo Directory { get; }
        

        //public Mod(IGame game, bool workshop, ModInfoFile modInfoFile) :
        //    base(game, workshop ? ModType.Workshops : ModType.Default, modInfoFile)
        //{
        //}

        public Mod(IGame game, DirectoryInfo modDirectory, bool workshop) :
            this(game, modDirectory, workshop, null)
        {
        }

        public Mod(IGame game, DirectoryInfo modDirectory, bool workshop, ModInfoData? modInfoData) :
            base(game, workshop ? ModType.Workshops : ModType.Default, modInfoData)
        {
            if (modDirectory is null)
                throw new ArgumentNullException(nameof(modDirectory));
            if (!modDirectory.Exists)
                throw new PetroglyphModException($"The mod's directory '{modDirectory.FullName}' does not exists.");
            Directory = modDirectory;
        }

        public override string ToArgs(bool includeDependencies)
        {
            if (includeDependencies)
                throw new NotImplementedException();

            var folderName = Directory.Name;
            return WorkshopMod ? $"STEAMMOD={folderName}" : $"MODPATH=Mods/{folderName}"; 
        }

        protected override bool ResolveDependenciesCore()
        {
            throw new NotImplementedException();
        }


        protected override string InitializeName()
        {
            var name = base.InitializeName();
            if (string.IsNullOrEmpty(name))
                name = Directory.Name;
            return name;
        }

        protected override string InitializeIcon()
        {
            var iconFile = base.InitializeIcon();
            if (!string.IsNullOrEmpty(iconFile))
                iconFile = Path.Combine(Directory.FullName, iconFile);
            else
            {
                var icon = Directory.EnumerateFiles("*.ico");
                iconFile = icon.FirstOrDefault()?.FullName;
            }
            return iconFile;
        }
    }
}
