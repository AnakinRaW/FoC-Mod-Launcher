using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading.Tasks;
using FocLauncher;

namespace FocLauncherHost.Utilities
{
    internal static class AssemblyExtractor
    {
        public static async Task WriteNecessaryAssembliesToDiskAsync(string fileDirectory, params string[] assemblyFiles)
        {
            foreach (var assemblyFile in assemblyFiles)
            {
                foreach (var resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                {
                    if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(resourceName, assemblyFile,
                            CompareOptions.IgnoreCase) < 0)
                        continue;
                    var compressed = resourceName.EndsWith(".compressed");
                    await WriteToFileAsync(resourceName, assemblyFile, fileDirectory, compressed);
                }
            }
        }

        public static void WriteNecessaryAssembliesToDisk(string fileDirectory, params string[] assemblyFiles)
        {
            Task.Run(async () => await WriteNecessaryAssembliesToDiskAsync(fileDirectory, assemblyFiles)).Wait();
        }

        private static async Task WriteToFileAsync(string resourceName, string matching, string fileDirectory, bool compressed)
        {
            if (!Directory.Exists(fileDirectory) || !PathUtilities.UserHasDirectoryAccessRights(fileDirectory, FileSystemRights.Modify))
                throw new IOException("The Launcher's base directory does not exists");
            var filePath = Path.Combine(fileDirectory, matching);
            try
            {
                using var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                if (rs == null)
                    throw new NullReferenceException(nameof(rs));
#if !DEBUG
                if (File.Exists(filePath))
                {
                    var resourceAssemblyBytes = rs.ToByteArray(Encoding.UTF8, true, true);
                    // TODO: Try avoid loading the assembly
                    var tmpAssembly = Assembly.ReflectionOnlyLoad(resourceAssemblyBytes);
                    var tmpVersion = tmpAssembly.GetName().Version;

                    if (File.ReadAllBytes(filePath).Length > 0)
                    {
                        var existingVersion = AssemblyName.GetAssemblyName(filePath).Version;
                        if (tmpVersion <= existingVersion)
                            return;
                    }
                }
#endif
                await WriteToFileAsync(rs, filePath, compressed);
            }
            catch (Exception ex)
            {
                throw new IOException("Error writing necessary launcher files to disk!", ex);
            }
        }

        private static async Task WriteToFileAsync(Stream assemblyStream, string filePath, bool decompress)
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            if (decompress)
            {
                using var decompressionStream = new DeflateStream(assemblyStream, CompressionMode.Decompress);
                var memoryStream = new MemoryStream();
                await decompressionStream.CopyToAsync(memoryStream);
                assemblyStream = memoryStream;
                assemblyStream.Position = 0;
            }
            await assemblyStream.CopyToAsync(fs);
        }
    }
}