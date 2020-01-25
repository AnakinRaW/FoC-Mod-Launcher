using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading.Tasks;
using FocLauncher;

namespace FocLauncherApp.Utilities
{
    internal static class AssemblyExtractor
    {
        public static void WriteNecessaryAssembliesToDisk(string fileDirectory, params string[] assemblyFiles)
        {
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var assemblyFile in assemblyFiles)
            {
                foreach (var resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                {
                    if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(resourceName, assemblyFile, CompareOptions.IgnoreCase) < 0)
                        continue;
                    WriteToFile(resourceName, assemblyFile, fileDirectory);
                }
            }
        }

        private static void WriteToFile(string resourceName, string matching, string fileDirectory)
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
                //Task.Run(async () => await WriteToFileAsync(rs, filePath)).Wait();
                WriteToFile(rs, filePath);
            }
            catch (Exception ex)
            {
                throw new IOException("Error writing necessary launcher files to disk!", ex);
            }
        }

        private static async Task WriteToFileAsync(Stream assemblyStream, string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await assemblyStream.CopyToAsync(fs);
        }

        private static void WriteToFile(Stream assemblyStream, string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            assemblyStream.CopyTo(fs);
        }
    }
}