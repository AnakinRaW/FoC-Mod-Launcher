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

        private static IMod CreateMod(IGame game, ModType type, DirectoryInfo directory, ModInfoData? modInfo, bool searchModFileOnDisk) 
        {
            if (game is null)
                throw new ArgumentNullException(nameof(game));
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));

            if (searchModFileOnDisk && modInfo is null)
            {
                var finder = new ModInfoFileFinder(directory);
                if (finder.TryFind(ModInfoFileFinder.FindOptions.FindMain, out var modInfoFile))
                    modInfo = modInfoFile.FirstOrDefault().GetModInfo();
            }
            
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

    public class ModInfoFileFinder
    {
        private readonly DirectoryInfo _directory;
        
        [Flags]
        public enum FindOptions
        {
            FindMain = 1,
            FindVariants = 2,
            FindAny = FindMain | FindVariants
        }


        private ModInfoFileFinder(DirectoryInfo directory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        /// <summary>
        /// Searches for a modinfo.json file on disk and creates a new <see cref="ModInfoFile"/> instance.
        /// </summary>
        /// <returns>A new instance of a <see cref="ModInfoFile"/></returns>
        /// <exception cref="FileNotFoundException">Throws <see cref="FileNotFoundException"/> when modinfo.json was not found.</exception>
        public static ModInfoFile FindModInfoFile(DirectoryInfo directory)
        {
            var result = Find(directory, FindOptions.FindMain);
            var modInfo = result.MainModInfo;
            if (modInfo is null)
                throw new FileNotFoundException($"No 'modinfo.json' found in the current directory: '{result.Directory.FullName}'");
            return modInfo;
        }

        public static ModInfoData FindModInfo(DirectoryInfo directory)
        {
            return FindModInfoFile(directory).GetModInfo();
        }

        public static IEnumerable<ModInfoFile> FindVariants(DirectoryInfo directory)
        {
            var result = Find(directory, FindOptions.FindVariants);
            var modInfo = result.MainModInfo;
            if (modInfo is null)
                throw new FileNotFoundException($"No 'modinfo.json' found in the current directory: '{result.Directory.FullName}'");
            return modInfo;
        }

        /// <summary>
        /// Based on <param name="options"></param> searches for a different kinds of modinfo json files on disk and creates a new <see cref="ModInfoFile"/> instance.
        /// </summary>
        /// <param name="directory">The directory where to search</param>
        /// <param name="options">The search options</param>
        /// <returns>A new instance of a <see cref="ModInfoFile"/></returns>
        /// <exception cref="FileNotFoundException">Throws <see cref="FileNotFoundException"/> when no modinfo.json was found.</exception>
        public static ModInfoFinderCollection Find(DirectoryInfo directory, FindOptions options)
        {
            var finder = new ModInfoFileFinder(directory);
            return finder.FindCore(options);
        }

        //public static bool TryFind(out ModInfoFile? modInfoFile)
        //{
        //    modInfoFile = default;
        //    if (!TryFind(FindOptions.FindMain, out var files))
        //        return false;
        //    modInfoFile = files.FirstOrDefault();
        //    return !(modInfoFile is null);
        //}

        //public static bool TryFindMany(FindOptions options, out ModInfoFinderCollection modInfoFiles)
        //{
        //    try
        //    {
        //        Find(options);
        //        return true;
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        return false;
        //    }
        //}

        private ModInfoFinderCollection FindCore(FindOptions options)
        {
            var result = new List<ModInfoFile>();

            if (options.HasFlag(FindOptions.FindMain))
            {
                var modinfoFile = _directory.EnumerateFiles(MainModInfoFileName, SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (modinfoFile is null || !IsValidModInfoFile(modinfoFile))
                {
                    if (options == FindOptions.FindMain)
                        throw new FileNotFoundException($"No 'modinfo.json' found in the current directory: '{_directory.FullName}'");
                    // Ignore if FindOptions.FindAny was selected.
                }
                else
                    result.Add(new ModInfoFile(modinfoFile));
            }
            if (options.HasFlag(FindOptions.FindVariants))
            {
                var possibleVariants =
                    _directory.EnumerateFiles($"*{VariantModInfoFileEnding}", SearchOption.TopDirectoryOnly).ToList();
                if (possibleVariants is null || !possibleVariants.Any())
                {
                    if (options == FindOptions.FindVariants)
                        throw new FileNotFoundException($"No modinfo variants found in the current directory: '{_directory.FullName}'");
                    // Ignore if FindOptions.FindAny was selected.
                }
                else
                {
                    result.AddRange(from variant in possibleVariants
                        where IsValidModInfoFile(variant)
                        select new ModInfoFile(variant));
                }
            }
            if (!result.Any())
                throw new FileNotFoundException($"No 'modinfo.json' or a variant found in the current directory: '{_directory.FullName}'");
            return result;
        }
    }

    public class ModInfoFinderCollection
    {
        public DirectoryInfo Directory { get; }
        public ModInfoFile? MainModInfo { get; }
        public IEnumerable<ModInfoFile> Variants { get; }

        public ModInfoFinderCollection(DirectoryInfo directory, ModInfoFile? mainModInfo, IEnumerable<ModInfoFile> variants)
        {
            Directory = directory;
            MainModInfo = mainModInfo;
            Variants = variants;
        }
    }
}
