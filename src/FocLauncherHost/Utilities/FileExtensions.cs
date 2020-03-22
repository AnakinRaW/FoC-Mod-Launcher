using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FocLauncherHost.Utilities
{
    internal static class FileExtensions
    {
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, bool recursive, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException(nameof(extensions));
            var files = dir.EnumerateFiles("*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            return files.Where(f => extensions.Contains(f.Extension));
        }
    }
}