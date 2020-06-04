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

        public static IEnumerable<IMod> CreateModAndVariants(IGame game, ModType type, DirectoryInfo directory, bool onlyVariantsIfPresent)
        {
            if (game is null)
                throw new ArgumentNullException(nameof(game));
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));

            if (!ModInfoFileFinder.TryFind(directory, ModInfoFileFinder.FindOptions.FindAny, out var modInfoCollection) || !modInfoCollection!.Variants.Any())
                yield return CreateModInstance(game, type, directory, modInfoCollection?.MainModInfo?.GetModInfo());

            if (modInfoCollection is null)
                yield break;

            if (!onlyVariantsIfPresent) 
                yield return CreateModInstance(game, type, directory, modInfoCollection.MainModInfo?.GetModInfo());

            foreach (var variant in modInfoCollection!.Variants)
                yield return CreateModInstance(game, type, directory, variant.GetModInfo());
        }

        public static IEnumerable<IMod> CreateModAndVariants(IGame game, ModType type, DirectoryInfo directory,
            bool onlyVariantsIfPresent, bool throwOnError)
        {
            if (game is null)
                throw new ArgumentNullException(nameof(game));
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));

            if (!ModInfoFileFinder.TryFind(directory, ModInfoFileFinder.FindOptions.FindAny, out var modInfoCollection))
            {
                // modInfoCollection is null: Just create a normal mod
                if (TryCreateModInstance(game, type, directory, null, out var mod))
                    yield return mod!;
            }

            if (!modInfoCollection!.Variants.Any())
            {

            }
                

            if (modInfoCollection is null)
                yield break;

            if (!onlyVariantsIfPresent)
                yield return CreateModInstance(game, type, directory, modInfoCollection.MainModInfo?.GetModInfo());

            foreach (var variant in modInfoCollection!.Variants)
                yield return CreateModInstance(game, type, directory, variant.GetModInfo());
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


        private static bool TryCreateModInstance(IGame game, ModType type, DirectoryInfo directory, ModInfoData? modInfo, out IMod? mod)
        {
            mod = default;
            try
            {
                mod = CreateModInstance(game, type, directory, modInfo);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
