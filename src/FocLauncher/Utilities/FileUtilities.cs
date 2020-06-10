using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FocLauncher.Utilities
{
    internal static class FileUtilities
    {
        internal static readonly char[] Slashes = { '/', '\\' };

        internal static readonly IEqualityComparer<string> Comparer = new PathComparer(false);
        internal static readonly IEqualityComparer<string> NormalizedComparer = new PathComparer(true);

        internal static bool IsDirectorySeparator(char c)
        {
            return Array.IndexOf(Slashes, c) >= 0;
        } 

        internal static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            var fullPath = GetFullPath(path);
            return FixFilePath(fullPath);
        }


        internal static string NormalizeForPathComparison(string path, bool getFullPath)
        {
            if (getFullPath)
                path = NormalizePath(path);
            return path.ToPlatformSlash().TrimTrailingSlashes();
        }

        private static bool PathsEqual(string path1, string path2)
        {
            return PathsEqual(path1, path2, Math.Max(path1.Length, path2.Length));
        }

        private static bool PathsEqual(string path1, string path2, int length)
        {
            if (path1.Length < length || path2.Length < length)
                return false;

            for (var i = 0; i < length; i++)
                if (!PathCharEqual(path1[i], path2[i]))
                    return false;

            return true;
        }

        private static bool PathCharEqual(char x, char y)
        {
            if (IsDirectorySeparator(x) && IsDirectorySeparator(y))
                return true;

            return  char.ToUpperInvariant(x) == char.ToUpperInvariant(y);
        }

        private static string ToPlatformSlash(this string s)
        {
            var separator = Path.DirectorySeparatorChar;
            return s.Replace(separator == '/' ? '\\' : '/', separator);
        }

        private static string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        private static string TrimTrailingSlashes(this string s)
        {
            return s.TrimEnd(Slashes);
        }

        private static int PathHashCode(string? path)
        {
            var hc = 0;
            if (path != null)
                hc = path.Where(ch => !IsDirectorySeparator(ch)).Aggregate(hc,
                    (current, ch) => Hash.Combine(char.ToUpperInvariant(ch), current));
            return hc;
        }


        internal static string FixFilePath(string path)
        {
            return string.IsNullOrEmpty(path) || Path.DirectorySeparatorChar == '\\' ? path : path.Replace('\\', '/');
        }


        private class PathComparer : IEqualityComparer<string?>
        {
            private readonly bool _applyNormalization;

            internal PathComparer(bool applyNormalization)
            {
                _applyNormalization = applyNormalization;
            }
            
            public bool Equals(string? x, string? y)
            {
                if (x == null && y == null)
                    return true;

                if (x == null || y == null)
                    return false;

                if (_applyNormalization)
                {
                    x = NormalizeForPathComparison(x, true);
                    y = NormalizeForPathComparison(y, true);
                }
                
                return PathsEqual(x, y);
            }

            public int GetHashCode(string? s)
            {
               return PathHashCode(s);
            }
        }

        private static class Hash
        {
            internal static int Combine(int newKey, int currentKey)
            {
                return unchecked((currentKey * (int)0xA5555529) + newKey);
            }
        }
    }
}
