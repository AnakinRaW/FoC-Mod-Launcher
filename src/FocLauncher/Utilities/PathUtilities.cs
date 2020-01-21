using System;
using System.IO;
using System.Text;

namespace FocLauncher.Utilities
{
    public static class PathUtilities
    {
        public static string NormalizePath(this string path)
        {
            return Normalize(path);
        }

        public static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                throw new ArgumentException(nameof(path));
            int index1 = 0;
            while (index1 < path.Length && char.IsWhiteSpace(path[index1]))
                ++index1;
            if (index1 == path.Length)
                return string.Empty;
            int index2 = path.Length - 1;
            while (index2 >= index1 && char.IsWhiteSpace(path[index2]))
                --index2;
            int num = index2 - index1 + 1;
            using (ReusableResourceHolder<StringBuilder> reusableResourceHolder = ReusableStringBuilder.AcquireDefault(260))
            {
                StringBuilder resource = reusableResourceHolder.Resource;
                bool flag1 = false;
                bool flag2 = false;
                for (int index3 = index1; index3 <= index2; ++index3)
                {
                    char c = path[index3];
                    if (c == Path.AltDirectorySeparatorChar)
                    {
                        c = Path.DirectorySeparatorChar;
                        flag2 = true;
                    }
                    if (c == Path.DirectorySeparatorChar)
                    {
                        if (flag1 && index3 > index1 + 1)
                        {
                            flag2 = true;
                            continue;
                        }
                    }
                    else if (char.IsUpper(c))
                    {
                        c = char.ToLower(c);
                        flag2 = true;
                    }
                    flag1 = c == Path.DirectorySeparatorChar;
                    resource.Append(c);
                }
                if (flag1 && resource.Length > 3)
                {
                    resource.Remove(resource.Length - 1, 1);
                    flag2 = true;
                }
                if (!flag2 && num == path.Length)
                    return path;
                return resource.ToString();
            }
        }
    }
}
