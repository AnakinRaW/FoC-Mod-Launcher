using System;
using System.Linq;
using FocLauncher.Core.Utilities;

namespace FocLauncher.Core.Versioning
{
    public sealed class VersionComparer : IVersionComparer
    {
        private readonly VersionComparison _mode;

        public static readonly IVersionComparer Default = new VersionComparer(VersionComparison.Default);
        public static readonly IVersionComparer Version = new VersionComparer(VersionComparison.Version);
        public static readonly IVersionComparer VersionRelease = new VersionComparer(VersionComparison.VersionRelease);
        public static IVersionComparer VersionReleaseMetadata = new VersionComparer(VersionComparison.VersionReleaseMetadata);

        public VersionComparer()
        {
            _mode = VersionComparison.Default;
        }

        public VersionComparer(VersionComparison versionComparison)
        {
            _mode = versionComparison;
        }

        public bool Equals(SemanticVersion x, SemanticVersion y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (_mode == VersionComparison.Default || _mode == VersionComparison.VersionRelease)
            {
                // Compare the version and release labels
                return (x.Major == y.Major
                        && x.Minor == y.Minor
                        && x.Patch == y.Patch
                        && GetRevisionOrZero(x) == GetRevisionOrZero(y)
                        && AreReleaseLabelsEqual(x, y));
            }

            // Use the full comparer for non-default scenarios
            return Compare(x, y) == 0;
        }

        public static int Compare(SemanticVersion version1, SemanticVersion version2, VersionComparison versionComparison)
        {
            var comparer = new VersionComparer(versionComparison);
            return comparer.Compare(version1, version2);
        }

        public int GetHashCode(SemanticVersion version)
        {
            if (ReferenceEquals(version, null))
            {
                return 0;
            }

            var combiner = new HashCodeCombiner();

            combiner.Add(version.Major);
            combiner.Add(version.Minor);
            combiner.Add(version.Patch);

            var modVersion = version as ModVersion;
            if (modVersion != null
                && modVersion.Revision > 0)
            {
                combiner.Add(modVersion.Revision);
            }

            if (_mode == VersionComparison.Default
                || _mode == VersionComparison.VersionRelease
                || _mode == VersionComparison.VersionReleaseMetadata)
            {
                var labels = GetReleaseLabelsOrNull(version);

                if (labels != null)
                {
                    var comparer = StringComparer.OrdinalIgnoreCase;
                    foreach (var label in labels)
                    {
                        combiner.Add(label, comparer);
                    }
                }
            }

            if (_mode == VersionComparison.VersionReleaseMetadata && version.HasMetadata)
            {
                combiner.Add(version.Metadata, StringComparer.OrdinalIgnoreCase);
            }

            return combiner.CombinedHash;
        }

        public int Compare(SemanticVersion x, SemanticVersion y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (ReferenceEquals(y, null))
            {
                return 1;
            }

            if (ReferenceEquals(x, null))
            {
                return -1;
            }

            // compare version
            var result = x.Major.CompareTo(y.Major);
            if (result != 0)
            {
                return result;
            }

            result = x.Minor.CompareTo(y.Minor);
            if (result != 0)
            {
                return result;
            }

            result = x.Patch.CompareTo(y.Patch);
            if (result != 0)
            {
                return result;
            }

            var legacyX = x as ModVersion;
            var legacyY = y as ModVersion;

            result = CompareLegacyVersion(legacyX, legacyY);
            if (result != 0)
            {
                return result;
            }

            if (_mode != VersionComparison.Version)
            {
                // compare release labels
                var xLabels = GetReleaseLabelsOrNull(x);
                var yLabels = GetReleaseLabelsOrNull(y);

                if (xLabels != null
                    && yLabels == null)
                {
                    return -1;
                }

                if (xLabels == null
                    && yLabels != null)
                {
                    return 1;
                }

                if (xLabels != null
                    && yLabels != null)
                {
                    result = CompareReleaseLabels(xLabels, yLabels);
                    if (result != 0)
                    {
                        return result;
                    }
                }

                // compare the metadata
                if (_mode == VersionComparison.VersionReleaseMetadata)
                {
                    result = StringComparer.OrdinalIgnoreCase.Compare(x.Metadata ?? string.Empty, y.Metadata ?? string.Empty);
                    if (result != 0)
                    {
                        return result;
                    }
                }
            }

            return 0;
        }

        private static int CompareLegacyVersion(ModVersion legacyX, ModVersion legacyY)
        {
            var result = 0;

            // true if one has a 4th version number
            if (legacyX != null
                && legacyY != null)
            {
                result = legacyX.Version.CompareTo(legacyY.Version);
            }
            else if (legacyX != null
                     && legacyX.Version.Revision > 0)
            {
                result = 1;
            }
            else if (legacyY != null
                     && legacyY.Version.Revision > 0)
            {
                result = -1;
            }

            return result;
        }

        private static int CompareReleaseLabels(string[] version1, string[] version2)
        {
            var result = 0;

            var count = Math.Max(version1.Length, version2.Length);

            for (var i = 0; i < count; i++)
            {
                var aExists = i < version1.Length;
                var bExists = i < version2.Length;

                if (!aExists && bExists)
                {
                    return -1;
                }

                if (aExists && !bExists)
                {
                    return 1;
                }

                // compare the labels
                result = CompareRelease(version1[i], version2[i]);

                if (result != 0)
                {
                    return result;
                }
            }

            return result;
        }

        private static int CompareRelease(string version1, string version2)
        {
            int result;

            // check if the identifiers are numeric
            var v1IsNumeric = int.TryParse(version1, out var version1Num);
            var v2IsNumeric = int.TryParse(version2, out var version2Num);

            // if both are numeric compare them as numbers
            if (v1IsNumeric && v2IsNumeric)
            {
                result = version1Num.CompareTo(version2Num);
            }
            else if (v1IsNumeric || v2IsNumeric)
            {
                // numeric labels come before alpha labels
                if (v1IsNumeric)
                {
                    result = -1;
                }
                else
                {
                    result = 1;
                }
            }
            else
            {
                // Ignoring 2.0.0 case sensitive compare. Everything will be compared case insensitively as 2.0.1 specifies.
                result = StringComparer.OrdinalIgnoreCase.Compare(version1, version2);
            }

            return result;
        }

        private static string[] GetReleaseLabelsOrNull(SemanticVersion version)
        {
            string[] labels = null;

            // Check if labels exist
            if (version.IsPrerelease)
            {
                // Try to use string[] which is how labels are normally stored.
                var enumerable = version.ReleaseLabels;
                labels = enumerable as string[];

                if (labels != null && enumerable != null)
                {
                    // This is not the expected type, enumerate and convert to an array.
                    labels = enumerable.ToArray();
                }
            }

            return labels;
        }

        private static bool AreReleaseLabelsEqual(SemanticVersion x, SemanticVersion y)
        {
            var xLabels = GetReleaseLabelsOrNull(x);
            var yLabels = GetReleaseLabelsOrNull(y);

            if (xLabels == null && yLabels != null)
            {
                return false;
            }

            if (xLabels != null && yLabels == null)
            {
                return false;
            }

            if (xLabels != null && yLabels != null)
            {
                // Both versions must have the same number of labels to be equal
                if (xLabels.Length != yLabels.Length)
                {
                    return false;
                }

                // Check if the labels are the same
                for (var i = 0; i < xLabels.Length; i++)
                {
                    if (!StringComparer.OrdinalIgnoreCase.Equals(xLabels[i], yLabels[i]))
                    {
                        return false;
                    }
                }
            }

            // labels are equal
            return true;
        }

        private static int GetRevisionOrZero(SemanticVersion version)
        {
            var modVersion = version as ModVersion;
            return modVersion?.Revision ?? 0;
        }

    }
}
