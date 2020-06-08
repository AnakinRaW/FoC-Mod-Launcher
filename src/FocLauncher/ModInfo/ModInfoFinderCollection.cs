using System.Collections.Generic;
using System.IO;

namespace FocLauncher.ModInfo
{
    public class ModInfoFinderCollection
    {
        public DirectoryInfo Directory { get; }
        public ModInfoFile? MainModInfo { get; }
        public IEnumerable<ModInfoVariantFile> Variants { get; }

        public ModInfoFinderCollection(DirectoryInfo directory, ModInfoFile? mainModInfo, IEnumerable<ModInfoVariantFile> variants)
        {
            Directory = directory;
            MainModInfo = mainModInfo;
            Variants = variants;
        }
    }
}