using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FocLauncher.ModInfo
{
    public class ModInfoFileFinder
    {
        private readonly DirectoryInfo _directory;
        private readonly ModInfoData? _baseModInfoData;

        [Flags]
        public enum FindOptions
        {
            FindMain = 1,
            FindVariants = 2,
            FindAny = FindMain | FindVariants
        }


        private ModInfoFileFinder(DirectoryInfo directory, ModInfoData? baseModInfoData)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _baseModInfoData = baseModInfoData;
        }

        /// <summary>
        /// Searches for a modinfo.json file on disk and creates a new <see cref="ModInfoFile"/> instance.
        /// </summary>
        /// <returns>A new instance of a <see cref="ModInfoFile"/></returns>
        /// <exception cref="FileNotFoundException">Throws <see cref="FileNotFoundException"/> when modinfo.json was not found.</exception>
        public static ModInfoFile FindModInfo(DirectoryInfo directory)
        {
            var result = Find(directory, FindOptions.FindMain);
            var modInfo = result.MainModInfo;
            if (modInfo is null)
                throw new FileNotFoundException($"No 'modinfo.json' found in the current directory: '{result.Directory.FullName}'");
            return modInfo;
        }
        
        public static IEnumerable<ModInfoVariantFile> FindVariants(DirectoryInfo directory)
        {
            var result = Find(directory, FindOptions.FindVariants);
            var variants = result.Variants.ToList();
            if (!variants.Any())
                throw new FileNotFoundException($"No 'modinfo.json' found in the current directory: '{result.Directory.FullName}'");
            return variants;
        }
        

        public static IEnumerable<ModInfoVariantFile> FindVariants(DirectoryInfo directory, ModInfoData mainModInfoData)
        {
            var result = Find(directory, FindOptions.FindVariants);
            var variants = result.Variants.ToList();
            if (!variants.Any())
                throw new FileNotFoundException($"No 'modinfo.json' found in the current directory: '{result.Directory.FullName}'");
            return variants;
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
            return Find(directory, options, null);
        }

        internal static ModInfoFinderCollection Find(DirectoryInfo directory, FindOptions options, ModInfoData? mainModInfoData)
        {
            var finder = new ModInfoFileFinder(directory, mainModInfoData);
            return finder.FindCore(options);
        }

        public static bool TryFindModInfo(DirectoryInfo directory, out ModInfoFile? modInfoFile)
        {
            modInfoFile = default;
            try
            {
                modInfoFile = FindModInfo(directory);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public static bool TryFindVariants(DirectoryInfo directory, out IEnumerable<ModInfoVariantFile> variants)
        {
            variants = Enumerable.Empty<ModInfoVariantFile>();
            try
            {
                variants = FindVariants(directory);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public static bool TryFindVariants(DirectoryInfo directory, ModInfoData mainModInfoData, out IEnumerable<ModInfoVariantFile> variants)
        {
            variants = Enumerable.Empty<ModInfoVariantFile>();
            try
            {
                variants = FindVariants(directory, mainModInfoData);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public static bool TryFind(DirectoryInfo directory, FindOptions options, out ModInfoFinderCollection? modInfoFiles)
        {
            modInfoFiles = default;
            try
            {
                modInfoFiles = Find(directory, options);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        private ModInfoFinderCollection FindCore(FindOptions options)
        {
            ModInfoFile? mainModInfoFile = default;
            List<ModInfoVariantFile> variantFiles = new List<ModInfoVariantFile>();
            if (options.HasFlag(FindOptions.FindMain))
            {
                mainModInfoFile = FindModInfoFileCore();
                ThrowFileNotFoundException($"No 'modinfo.json' found in the current directory: '{_directory.FullName}'", 
                    () => mainModInfoFile is null && options == FindOptions.FindMain);
            }
            if (options.HasFlag(FindOptions.FindVariants))
            {
                variantFiles.AddRange(FindModInfoVariantFilesCore(mainModInfoFile?.GetModInfo() ?? _baseModInfoData));
                ThrowFileNotFoundException($"No modinfo variants found in the current directory: '{_directory.FullName}'",
                    () => !variantFiles.Any() && options == FindOptions.FindVariants);
            }

            ThrowFileNotFoundException($"No 'modinfo.json' or a variant found in the current directory: '{_directory.FullName}'",
                () => mainModInfoFile is null && !variantFiles.Any());

            return new ModInfoFinderCollection(_directory, mainModInfoFile, variantFiles);
        }

        private ModInfoFile? FindModInfoFileCore()
        {
            var file = _directory.EnumerateFiles(ModInfoFile.ModInfoFileName, SearchOption.TopDirectoryOnly).FirstOrDefault();
            return file is null ? null : new ModInfoFile(file);
        }
         
        private IEnumerable<ModInfoVariantFile> FindModInfoVariantFilesCore(ModInfoData? mainModInfoFile)
        {
            var possibleVariants = _directory.EnumerateFiles($"*{ModInfoVariantFile.VariantModInfoFileEnding}", SearchOption.TopDirectoryOnly).ToList();
            return from possibleVariant in possibleVariants select new ModInfoVariantFile(possibleVariant, mainModInfoFile);
        }

        private static void ThrowFileNotFoundException(string message, Func<bool> throwCondition)
        {
            if (throwCondition())
                throw new FileNotFoundException(message);
        }
    }
}