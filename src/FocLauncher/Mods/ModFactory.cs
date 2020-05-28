using System;
using System.IO;
using FocLauncher.Game;
using FocLauncher.ModInfo;

namespace FocLauncher.Mods
{
    public static class ModFactory
    {
        public static IMod CreateMod(IGame game, ModType type, DirectoryInfo directory, ModInfoData modInfo)
        {
            return CreateMod(game, type, directory, modInfo, false);
        }

        public static IMod CreateMod(IGame game, ModType type, DirectoryInfo directory, bool searchModFileOnDisk)
        {
            return CreateMod(game, type, directory, null, searchModFileOnDisk);
        }

        public static IMod CreateMod(IGame game, ModType type, string modPath, ModInfoData modInfo)
        {
            return CreateMod(game, type, new DirectoryInfo(modPath), modInfo);
        }

        public static IMod CreateMod(IGame game, ModType type, string modPath, bool searchModFileOnDisk)
        {
            return CreateMod(game, type, new DirectoryInfo(modPath), searchModFileOnDisk);
        }

        private static IMod CreateMod(IGame game, ModType type, DirectoryInfo directory, ModInfoData? modInfo, bool searchModFileOnDisk) 
        {
            if (game is null)
                throw new ArgumentNullException(nameof(game));
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));

            if (searchModFileOnDisk && modInfo is null && ModInfoFile.Find(directory, out var modInfoFile))
                modInfo = modInfoFile!.GetModInfo();

            switch (type)
            {
                case ModType.Default:
                case ModType.Workshops:
                    return new Mod(game, directory, type == ModType.Workshops, modInfo);
                case ModType.Virtual:
                    return new VirtualMod(game, modInfo);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
