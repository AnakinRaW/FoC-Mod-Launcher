using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FocLauncher.Core.Versioning
{
    public class ModVersion : SemanticVersion
    {
        public Version Version => _version;

        public virtual bool IsLegacyVersion => Version.Revision > 0;

        public int Revision => _version.Revision;

        public bool IsSemVer2 => _releaseLabels != null && _releaseLabels.Length > 1 || HasMetadata;

        public string OriginalVersion { get; }


        public ModVersion(string version)
            : this(Parse(version))
        {
        }

        public ModVersion(ModVersion version)
            : this(version.Version, version.ReleaseLabels, version.Metadata, version.OriginalVersion)
        {
        }

        public ModVersion(Version version, string releaseLabel = null, string metadata = null)
            : this(version, ParseReleaseLabels(releaseLabel), metadata, GetLegacyString(version, ParseReleaseLabels(releaseLabel), metadata))
        {
        }

        public ModVersion(int major, int minor, int patch)
            : this(major, minor, patch, EmptyReleaseLabels, null)
        {
        }

        public ModVersion(int major, int minor, int patch, string releaseLabel)
            : this(major, minor, patch, ParseReleaseLabels(releaseLabel), null)
        {
        }

        public ModVersion(int major, int minor, int patch, string releaseLabel, string metadata)
            : this(major, minor, patch, ParseReleaseLabels(releaseLabel), metadata)
        {
        }

        public ModVersion(int major, int minor, int patch, IEnumerable<string> releaseLabels, string metadata)
            : this(new Version(major, minor, patch, 0), releaseLabels, metadata, null)
        {
        }

        public ModVersion(int major, int minor, int patch, int revision)
            : this(major, minor, patch, revision, EmptyReleaseLabels, null)
        {
        }

        public ModVersion(int major, int minor, int patch, int revision, string releaseLabel, string metadata)
            : this(major, minor, patch, revision, ParseReleaseLabels(releaseLabel), metadata)
        {
        }

        public ModVersion(int major, int minor, int patch, int revision, IEnumerable<string> releaseLabels, string metadata)
            : this(new Version(major, minor, patch, revision), releaseLabels, metadata, null)
        {
        }

        public ModVersion(Version version, IEnumerable<string> releaseLabels, string metadata, string originalVersion)
            : base(version, releaseLabels, metadata)
        {
            OriginalVersion = originalVersion;
        }

        public new static ModVersion Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException();

            if (!TryParse(value, out var ver))
                throw new ArgumentException();

            return ver;
        }

        public static bool TryParse(string value, out ModVersion version)
        {
            version = null;

            if (value != null)
            {
                // trim the value before passing it in since we not strict here
                var sections = ParseSections(value.Trim());

                // null indicates the string did not meet the rules
                if (sections != null
                    && !string.IsNullOrEmpty(sections.Item1))
                {
                    var versionPart = sections.Item1;

                    if (versionPart.IndexOf('.') < 0)
                        // System.Version requires at least a 2 part version to parse.
                        versionPart += ".0";

                    if (Version.TryParse(versionPart, out var systemVersion))
                    {
                        // labels
                        if (sections.Item2 != null)
                        {
                            if (sections.Item2.Any(t => !IsValidPart(t, false)))
                                return false;
                        }

                        // build metadata
                        if (sections.Item3 != null
                            && !IsValid(sections.Item3, true))
                            return false;

                        var ver = NormalizeVersionValue(systemVersion);

                        var originalVersion = value;

                        if (originalVersion.IndexOf(' ') > -1) originalVersion = value.Replace(" ", string.Empty);

                        version = new ModVersion(ver,
                            sections.Item2,
                            sections.Item3 ?? string.Empty,
                            originalVersion);

                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryParseStrict(string value, out ModVersion version)
        {
            version = null;

            if (TryParse(value, out SemanticVersion semVer))
                version = new ModVersion(semVer.Major, semVer.Minor, semVer.Patch, 0, semVer.ReleaseLabels,
                    semVer.Metadata);

            return true;
        }

        
        public override string ToString()
        {
            if (string.IsNullOrEmpty(OriginalVersion) || IsSemVer2)
                return ToNormalizedString();
            return OriginalVersion;
        }

        private static IEnumerable<string> ParseReleaseLabels(string releaseLabels)
        {
            return !string.IsNullOrEmpty(releaseLabels) ? releaseLabels.Split('.') : null;
        }

        private static string GetLegacyString(Version version, IEnumerable<string> releaseLabels, string metadata)
        {
            var sb = new StringBuilder(version.ToString());

            if (releaseLabels != null)
                sb.AppendFormat(CultureInfo.InvariantCulture, "-{0}", string.Join(".", releaseLabels));

            if (!string.IsNullOrEmpty(metadata))
                sb.AppendFormat(CultureInfo.InvariantCulture, "+{0}", metadata);

            return sb.ToString();
        }


    }
}
