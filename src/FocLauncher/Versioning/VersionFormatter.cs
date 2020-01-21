using System;
using System.Globalization;
using System.Text;

namespace FocLauncher.Versioning
{
    public class VersionFormatter : IFormatProvider, ICustomFormatter
    {
        public static readonly VersionFormatter Instance = new VersionFormatter();

        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter) ||
                formatType == typeof(SemanticVersion))
                return this;
            return null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null)
                throw new ArgumentNullException(nameof(arg));

            string formatted = null;
            var argType = arg.GetType();

            if (argType == typeof(IFormattable))
                formatted = ((IFormattable) arg).ToString(format, formatProvider);
            else if (!string.IsNullOrEmpty(format))
            {
                if (arg is SemanticVersion version)
                {
                    if (format.Length == 1)
                        formatted = Format(format[0], version);
                    else
                    {
                        var sb = new StringBuilder(format.Length);

                        foreach (var t in format)
                        {
                            var s = Format(t, version);

                            if (s == null)
                                sb.Append(t);
                            else
                                sb.Append(s);
                        }

                        formatted = sb.ToString();
                    }
                }
            }

            return formatted;
        }

        private static string Format(char c, SemanticVersion version)
        {
            string s = null;

            switch (c)
            {
                case 'N':
                    s = GetNormalizeString(version);
                    break;
                case 'R':
                    s = version.Release;
                    break;
                case 'M':
                    s = version.Metadata;
                    break;
                case 'V':
                    s = FormatVersion(version);
                    break;
                case 'F':
                    s = GetFullString(version);
                    break;
                case 'x':
                    s = string.Format(CultureInfo.InvariantCulture, "{0}", version.Major);
                    break;
                case 'y':
                    s = string.Format(CultureInfo.InvariantCulture, "{0}", version.Minor);
                    break;
                case 'z':
                    s = string.Format(CultureInfo.InvariantCulture, "{0}", version.Patch);
                    break;
                case 'r':
                    var modVersion = version as ModVersion;
                    s = string.Format(CultureInfo.InvariantCulture, "{0}", modVersion != null && modVersion.IsLegacyVersion ? modVersion.Version.Revision : 0);
                    break;
            }

            return s;
        }

        private static string GetNormalizeString(SemanticVersion version)
        {
            var normalized = Format('V', version);
            if (version.IsPrerelease)
                normalized = $"{normalized}-{version.Release}";
            return normalized;
        }

        private static string GetFullString(SemanticVersion version)
        {
            var fullString = GetNormalizeString(version);

            if (version.HasMetadata)
                fullString = $"{fullString}+{version.Metadata}";
            return fullString;
        }

        private static string FormatVersion(SemanticVersion version)
        {
            var modVersion = version as ModVersion;
            var legacy = modVersion != null && modVersion.IsLegacyVersion;

            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}{3}", version.Major, version.Minor,
                version.Patch,
                legacy
                    ? string.Format(CultureInfo.InvariantCulture, ".{0}", modVersion.Version.Revision)
                    : null);
        }
    }
}
