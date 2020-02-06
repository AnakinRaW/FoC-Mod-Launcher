using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;

namespace FocLauncherHost.Updater
{
    internal static class UpdaterUtilities
    {
        internal static Version? GetAssemblyVersion(string file)
        {
            if (!File.Exists(file))
                return null;
            var assemblyName = AssemblyName.GetAssemblyName(file);
            return assemblyName.Version;
        }

        internal static byte[]? GetSha2(string file)
        {
            if (!File.Exists(file))
                return null;
            using var sha2 = SHA256.Create();
            var fileInfo = new FileInfo(file);
            using var fs = fileInfo.OpenRead();
            fs.Position = 0;
            return sha2.ComputeHash(fs);
        }

        // https://stackoverflow.com/a/9995303
        internal static byte[] HexToArray(string input)
        {
            if (input.Length % 2 != 0)
                throw new NotSupportedException("string input lenght is wrong");
            input = input.ToLower();
            var arr = new byte[input.Length >> 1];
            for (var i = 0; i < input.Length >> 1; ++i)
                arr[i] = (byte)((GetHexVal(input[i << 1]) << 4) + (GetHexVal(input[(i << 1) + 1])));
            return arr;
        }

        internal static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                extensions = new[] {".*"};
            var files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }

        internal static DependencyRemoveResult TryRemoveDependency(DependencyCheckResult task, CancellationTokenSource cts)
        {
            //cts.Cancel();
            return new DependencyRemoveResult(task.Dependency, UpdateActionResult.Success);
        }

        private static int GetHexVal(char hex)
        {
            var val = (int)hex;
            return val - (val < 58 ? 48 : 87);
        }
    }
}
