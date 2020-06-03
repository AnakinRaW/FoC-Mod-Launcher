using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static IEnumerable<IMod> CreateModAndVariants(IGame game, ModType type, DirectoryInfo directory)
        {
            if (game is null)
                throw new ArgumentNullException(nameof(game));
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));

            if (!ModInfoFileFinder.TryFind(directory, ModInfoFileFinder.FindOptions.FindAny, out var modInfoCollection))
                return new List<IMod> {CreateModInstance(game, type, directory)};

            var result = new List<IMod>();
            result.Add(CreateModInstance(game, type, directory, modInfoCollection?.MainModInfo?.GetModInfo()));
            result.AddRange(modInfoCollection!.Variants.Select(variant => CreateModInstance(game, type, directory, variant.GetModInfo())));
            return result;
        }

        private static IMod CreateMod(IGame game, ModType type, DirectoryInfo directory, ModInfoData? modInfo, bool searchModFileOnDisk) 
        {
            if (game is null)
                throw new ArgumentNullException(nameof(game));
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));

            if (searchModFileOnDisk && modInfo is null)
            {
                if (ModInfoFileFinder.TryFindModInfo(directory, out var modInfoFile))
                    modInfo = modInfoFile!.GetModInfo();
            }


            return CreateModInstance(game, type, directory, modInfo);
        }

        private static IMod CreateModInstance(IGame game, ModType type, DirectoryInfo directory, ModInfoData? modInfo = null)
        {
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
