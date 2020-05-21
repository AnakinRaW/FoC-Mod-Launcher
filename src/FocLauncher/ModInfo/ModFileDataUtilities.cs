using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FocLauncher.ModInfo
{
    public static class ModFileDataUtilities
    {
        internal static void CheckModInfoFile(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                throw new FileNotFoundException("The file was not found!");
            if (!fileInfo.Name.ToLowerInvariant().Equals("modinfo.json"))
                throw new ModInfoException("The file's name must be 'modinfo.json'.");
        }

        public static ModInfoData Parse(FileInfo fileInfo)
        {
            if (fileInfo is null)
                throw new ArgumentNullException(nameof(fileInfo));
            CheckModInfoFile(fileInfo);
            var text = File.ReadAllText(fileInfo.FullName);
            var modInfo = JsonConvert.DeserializeObject<ModInfoData>(text);
            if (string.IsNullOrWhiteSpace(modInfo.Name))
                throw new ModInfoException("No mod name was specified");
            return modInfo;
        }

        public static async Task<ModInfoData> ParseAsync(FileInfo fileInfo)
        {
            if (fileInfo is null)
                throw new ArgumentNullException(nameof(fileInfo));
            CheckModInfoFile(fileInfo);
            var text = await ReadTextAsync(fileInfo).ConfigureAwait(false);
            var modInfo = await Task.Run(() => JsonConvert.DeserializeObject<ModInfoData>(text)).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(modInfo.Name))
                throw new ModInfoException("No mod name was specified");
            return modInfo;
        }

        private static async Task<string> ReadTextAsync(FileSystemInfo fileInfo)
        {
            using var sourceStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            var sb = new StringBuilder();

            var buffer = new byte[0x1000];
            int numRead;
            while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                var text = Encoding.Unicode.GetString(buffer, 0, numRead);
                sb.Append(text);
            }
            return sb.ToString();
        }
    }
}