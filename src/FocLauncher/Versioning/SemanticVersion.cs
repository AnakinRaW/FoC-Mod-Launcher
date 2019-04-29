using System;
using System.Collections.Generic;
using System.Linq;

namespace FocLauncher.Core.Versioning
{
    public class SemanticVersion : IFormattable, IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {

        internal static readonly string[] EmptyReleaseLabels = Array.Empty<string>();

        internal readonly string[] _releaseLabels;
        internal readonly string _metadata;
        internal readonly Version _version;

        public int Major => _version.Major;

        public int Minor => _version.Minor;

        public int Patch => _version.Build;

        public IEnumerable<string> ReleaseLabels => _releaseLabels ?? EmptyReleaseLabels;

        public string Release
        {
            get
            {
                if (_releaseLabels == null)
                    return string.Empty;
                return _releaseLabels.Length == 1 ? _releaseLabels[0] : string.Join(".", _releaseLabels);
            }
        }

        public virtual bool IsPrerelease
        {
            get
            {
                return _releaseLabels != null && _releaseLabels.Any(t => !string.IsNullOrEmpty(t));
            }
        }

        public SemanticVersion(SemanticVersion version)
            : this(version.Major, version.Minor, version.Patch, version.ReleaseLabels, version.Metadata)
        {
        }

        public SemanticVersion(int major, int minor, int patch)
            : this(major, minor, patch, Enumerable.Empty<string>(), null)
        {
        }

        public SemanticVersion(int major, int minor, int patch, string releaseLabel)
            : this(major, minor, patch, ParseReleaseLabels(releaseLabel), null)
        {
        }

        public SemanticVersion(int major, int minor, int patch, string releaseLabel, string metadata)
            : this(major, minor, patch, ParseReleaseLabels(releaseLabel), metadata)
        {
        }

        public SemanticVersion(int major, int minor, int patch, IEnumerable<string> releaseLabels, string metadata)
            : this(new Version(major, minor, patch, 0), releaseLabels, metadata)
        {
        }

        protected SemanticVersion(Version version, string releaseLabel = null, string metadata = null)
            : this(version, ParseReleaseLabels(releaseLabel), metadata)
        {
        }

        protected SemanticVersion(int major, int minor, int patch, int revision, string releaseLabel, string metadata)
            : this(major, minor, patch, revision, ParseReleaseLabels(releaseLabel), metadata)
        {
        }

        protected SemanticVersion(int major, int minor, int patch, int revision, IEnumerable<string> releaseLabels, string metadata)
            : this(new Version(major, minor, patch, revision), releaseLabels, metadata)
        {
        }

        protected SemanticVersion(Version version, IEnumerable<string> releaseLabels, string metadata)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            _version = NormalizeVersionValue(version);
            _metadata = metadata;

            if (releaseLabels != null)
            {
                // If the labels are already an array use it

                if (releaseLabels is string[] asArray)
                    _releaseLabels = asArray;
                else
                    _releaseLabels = releaseLabels.ToArray();

                if (_releaseLabels.Length < 1)
                    _releaseLabels = null;
            }
        }


        public virtual bool HasMetadata => !string.IsNullOrEmpty(Metadata);

        public virtual string Metadata => _metadata;

        public virtual string ToNormalizedString()
        {
            return ToString("N", VersionFormatter.Instance);
        }

        public virtual string ToFullString()
        {
            return ToString("F", VersionFormatter.Instance);
        }

        public override string ToString()
        {
            return ToNormalizedString();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider == null || !TryFormatter(format, formatProvider, out string formattedString))
                formattedString = ToString();
            return formattedString;
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as SemanticVersion);
        }

        public int CompareTo(SemanticVersion other)
        {
            return CompareTo(other, VersionComparison.Default);
        }

        public virtual int CompareTo(SemanticVersion other, VersionComparison versionComparison)
        {
            var comparer = new VersionComparer(versionComparison);
            return comparer.Compare(this, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SemanticVersion);
        }

        public override int GetHashCode()
        {
            return VersionComparer.Default.GetHashCode(this);
        }

        public bool Equals(SemanticVersion other)
        {
            return VersionComparer.Default.Equals(this, other);
        }

        public static SemanticVersion Parse(string value)
        {
            if (!TryParse(value, out var ver))
                throw new ArgumentException();

            return ver;
        }

        public static bool TryParse(string value, out SemanticVersion version)
        {
            version = null;

            if (value != null)
            {
                var sections = ParseSections(value);

                // null indicates the string did not meet the rules
                if (sections != null
                    && Version.TryParse(sections.Item1, out var systemVersion))
                {
                    // validate the version string
                    var parts = sections.Item1.Split('.');

                    if (parts.Length != 3)
                        return false;

                    if (parts.Any(part => !IsValidPart(part, false)))
                        return false;

                    // labels
                    if (sections.Item2 != null)
                        if (sections.Item2.Any(t => !IsValidPart(t, false)))
                            return false;

                    // build metadata
                    if (sections.Item3 != null
                        && !IsValid(sections.Item3, true))
                        return false;

                    var ver = NormalizeVersionValue(systemVersion);

                    version = new SemanticVersion(ver,
                        sections.Item2,
                        sections.Item3 ?? string.Empty);

                    return true;
                }
            }
            return false;
        }

        internal static Tuple<string, string[], string> ParseSections(string value)
        {
            string versionString = null;
            string[] releaseLabels = null;
            string buildMetadata = null;

            var dashPos = -1;
            var plusPos = -1;

            for (var i = 0; i < value.Length; i++)
            {
                var end = (i == value.Length - 1);

                if (dashPos < 0)
                {
                    if (!end && value[i] != '-' && value[i] != '+')
                        continue;
                    var endPos = i + (end ? 1 : 0);
                    versionString = value.Substring(0, endPos);

                    dashPos = i;

                    if (value[i] == '+')
                    {
                        plusPos = i;
                    }
                }
                else if (plusPos < 0)
                {
                    if (!end && value[i] != '+')
                        continue;
                    var start = dashPos + 1;
                    var endPos = i + (end ? 1 : 0);
                    var releaseLabel = value.Substring(start, endPos - start);

                    releaseLabels = releaseLabel.Split('.');

                    plusPos = i;
                }
                else if (end)
                {
                    var start = plusPos + 1;
                    var endPos = i + 1;
                    buildMetadata = value.Substring(start, endPos - start);
                }
            }

            return new Tuple<string, string[], string>(versionString, releaseLabels, buildMetadata);
        }

        internal static Version NormalizeVersionValue(Version version)
        {
            var normalized = version;

            if (version.Build < 0
                || version.Revision < 0)
            {
                normalized = new Version(
                    version.Major,
                    version.Minor,
                    Math.Max(version.Build, 0),
                    Math.Max(version.Revision, 0));
            }

            return normalized;
        }

        private static string[] ParseReleaseLabels(string releaseLabels)
        {
            if (!string.IsNullOrEmpty(releaseLabels))
            {
                return releaseLabels.Split('.');
            }

            return null;
        }

        internal static bool IsLetterOrDigitOrDash(char c)
        {
            var x = (int)c;

            return x >= 48 && x <= 57 || x >= 65 && x <= 90 || x >= 97 && x <= 122 || x == 45;
        }

        internal static bool IsDigit(char c)
        {
            var x = (int)c;
            return (x >= 48 && x <= 57);
        }

        internal static bool IsValid(string s, bool allowLeadingZeros)
        {
            var parts = s.Split('.');

            // Check each part individually
            return parts.All(t => IsValidPart(t, allowLeadingZeros));
        }

        internal static bool IsValidPart(string s, bool allowLeadingZeros)
        {
            if (s.Length == 0)
            {
                // empty labels are not allowed
                return false;
            }

            // 0 is fine, but 00 is not. 
            // 0A counts as an alpha numeric string where zeros are not counted
            if (!allowLeadingZeros
                && s.Length > 1
                && s[0] == '0')
            {
                var allDigits = true;

                // Check if all characters are digits.
                // The first is already checked above
                for (int i = 1; i < s.Length; i++)
                {
                    if (IsDigit(s[i]))
                        continue;
                    allDigits = false;
                    break;
                }

                if (allDigits)
                {
                    // leading zeros are not allowed in numeric labels
                    return false;
                }
            }

            return s.All(IsLetterOrDigitOrDash);
        }

        public static bool operator ==(SemanticVersion version1, SemanticVersion version2)
        {
            return Equals(version1, version2);
        }

        /// <summary>
        /// Not equal
        /// </summary>
        public static bool operator !=(SemanticVersion version1, SemanticVersion version2)
        {
            return !Equals(version1, version2);
        }

        /// <summary>
        /// Less than
        /// </summary>
        public static bool operator <(SemanticVersion version1, SemanticVersion version2)
        {
            return Compare(version1, version2) < 0;
        }

        /// <summary>
        /// Less than or equal
        /// </summary>
        public static bool operator <=(SemanticVersion version1, SemanticVersion version2)
        {
            return Compare(version1, version2) <= 0;
        }

        /// <summary>
        /// Greater than
        /// </summary>
        public static bool operator >(SemanticVersion version1, SemanticVersion version2)
        {
            return Compare(version1, version2) > 0;
        }

        /// <summary>
        /// Greater than or equal
        /// </summary>
        public static bool operator >=(SemanticVersion version1, SemanticVersion version2)
        {
            return Compare(version1, version2) >= 0;
        }

        protected bool TryFormatter(string format, IFormatProvider formatProvider, out string formattedString)
        {
            var formatted = false;
            formattedString = null;

            if (formatProvider?.GetFormat(GetType()) is ICustomFormatter formatter)
            {
                formatted = true;
                formattedString = formatter.Format(format, this, formatProvider);
            }

            return formatted;
        }

        private static int Compare(SemanticVersion version1, SemanticVersion version2)
        {
            return VersionComparer.Default.Compare(version1, version2);
        }
    }
}
