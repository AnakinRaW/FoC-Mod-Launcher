using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

    public class ModInfoFile
    {
        public const string ModInfoFileName = "modinfo.json";

        private DateTime? _lastWriteTime;
        private ModInfoData? _data;

        public bool IsValid
        {
            get
            {
                try
                {
                    Validate();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public FileInfo File { get; }

        public ModInfoFile(FileInfo modInfoFile)
        {
            File = modInfoFile ?? throw new ArgumentNullException(nameof(modInfoFile));
            _lastWriteTime = modInfoFile.LastWriteTime;
        }
        
        public virtual void Validate()
        {
            if (!File.Exists)
                throw new ModInfoException($"ModInfo file does not exists at '{File.FullName}'.");
            if (!File.Name.ToUpperInvariant().Equals(ModInfoFileName.ToUpperInvariant()))
                throw new ModInfoException("The file's name must be 'modinfo.json'.");
        }

        public void Invalidate()
        {
            Validate();
            _lastWriteTime = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ModInfoException">Throws if it was not possible to get the <see cref="ModInfoData"/> or the result was not valid.</exception>
        public async Task<ModInfoData> GetModInfoAsync()
        {
            Validate();
            if (_data != null && _lastWriteTime.HasValue && _lastWriteTime.Value.Equals(File.LastWriteTime))
                return _data;
            _data = await GetModInfoCoreAsync().ConfigureAwait(false);
            _data.Validate();
            return _data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ModInfoException">Throws if it was not possible to get the <see cref="ModInfoData"/> or the result was not valid.</exception>
        public ModInfoData GetModInfo()
        {
            Validate();
            if (_data != null && _lastWriteTime.HasValue && _lastWriteTime.Value.Equals(File.LastWriteTime))
                return _data;
            _data = GetModInfoCore();
            _data.Validate();
            return _data;
        }

        public ModInfoData? TryGetModInfo()
        {
            try
            { 
                return GetModInfo();
            }
            catch
            {
                return null;
            }
        }

        protected virtual Task<ModInfoData> GetModInfoCoreAsync()
        {
            return ParseAsync();
        }

        protected virtual ModInfoData GetModInfoCore()
        {
            return Parse();
        }

        protected async Task<ModInfoData> ParseAsync()
        {
            var text = await ReadTextAsync(File).ConfigureAwait(false);
            return await Task.Run(() => JsonConvert.DeserializeObject<ModInfoData>(text)).ConfigureAwait(false);
        }

        protected ModInfoData Parse()
        {
            var text = System.IO.File.ReadAllText(File.FullName);
            return JsonConvert.DeserializeObject<ModInfoData>(text);
        }

        private static async Task<string> ReadTextAsync(FileSystemInfo fileInfo)
        {
            using var sourceStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            var sb = new StringBuilder();

            var buffer = new byte[0x1000];
            int numRead;
            while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) != 0)
            {
                var text = Encoding.Unicode.GetString(buffer, 0, numRead);
                sb.Append(text);
            }
            return sb.ToString();
        }
    }
}
