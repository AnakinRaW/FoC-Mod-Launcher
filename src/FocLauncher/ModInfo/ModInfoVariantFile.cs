using System;
using System.IO;
using System.Threading.Tasks;

namespace FocLauncher.ModInfo
{
    public sealed class ModInfoVariantFile : ModInfoFile
    {
        public const string VariantModInfoFileEnding = "-modinfo.json";

        private readonly ModInfoFile? _mainModInfoFile;
        private ModInfoData? _mainModInfoData;

        public ModInfoVariantFile(FileInfo variant) : base(variant)
        {
        }

        public ModInfoVariantFile(FileInfo variant, ModInfoFile? mainModInfoFile) : base(variant)
        {
            if (mainModInfoFile is ModInfoVariantFile)
                throw new ModInfoException("A ModInfoFile's base must not be a variant file too.");
            _mainModInfoFile = mainModInfoFile;
        }

        public ModInfoVariantFile(FileInfo variant, ModInfoData? mainModInfoData) : base(variant)
        {
            _mainModInfoData = mainModInfoData;
        }

        public override void Validate()
        {
            if (!File.Exists)
                throw new ModInfoException($"ModInfo variant file does not exists at '{File.FullName}'.");
            if (!File.Name.ToUpperInvariant().EndsWith(VariantModInfoFileEnding.ToUpperInvariant(), StringComparison.InvariantCultureIgnoreCase))
                throw new ModInfoException("The file's name must end with '-modinfo.json'.");
        }

        protected override async Task<ModInfoData> GetModInfoCoreAsync()
        {
            var data = await ParseAsync();
            if (_mainModInfoData is null && _mainModInfoFile != null)
                _mainModInfoData = await _mainModInfoFile.GetModInfoAsync().ConfigureAwait(false);
            return data.Merge(_mainModInfoData);
        }

        protected override ModInfoData GetModInfoCore()
        {
            var data = Parse();
            if (_mainModInfoData is null && _mainModInfoFile != null)
                _mainModInfoData = _mainModInfoFile.GetModInfo();
            return data.Merge(_mainModInfoData);
        }
    }
}