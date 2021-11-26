using System.Collections.Generic;
using System.IO;
using System.Linq;
using FocLauncher;
using NLog;

namespace MetadataCreator
{
    internal static class FileUtilities
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, bool includeSubs = false, params string[] extensions)
        {
            if (extensions == null)
                extensions = new[] { ".*" };
            var files = dir.EnumerateFiles("*.*", includeSubs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            return files.Where(f => extensions.Contains(f.Extension));
        }

        internal static IEnumerable<FileInfo> GetApplicationFiles(IReadOnlyCollection<FileInfo> files, string buildType)
        {
            Logger.Trace($"Searching application files for {buildType}");
            foreach (var fileName in LauncherConstants.ApplicationFileNames)
            {
                var foundFile = files.FirstOrDefault(x =>
                    x.Name.Equals(fileName) && x.Directory != null && x.Directory.Name.Equals(buildType));
                if (foundFile is null)
                    throw new FileNotFoundException($"File '{fileName}' was not found as {buildType}-Build");
                Logger.Info($"Found application file: {foundFile.Name}");
                yield return foundFile;
            }
        }
    }
}
