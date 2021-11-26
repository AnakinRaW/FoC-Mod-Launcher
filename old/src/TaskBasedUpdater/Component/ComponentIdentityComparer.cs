﻿using System;
using System.Collections.Generic;

namespace TaskBasedUpdater.Component
{
    public class ComponentIdentityComparer : IEqualityComparer<IComponent>, IComparer<IComponent>
    {
        public static readonly ComponentIdentityComparer Default = new ComponentIdentityComparer();
        public static readonly ComponentIdentityComparer VersionIndependent = new ComponentIdentityComparer(true);

        private readonly bool _excludeVersion;
        private readonly StringComparison _comparisonType;
        private readonly StringComparer _comparer;

        public ComponentIdentityComparer(bool excludeVersion = false, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            _excludeVersion = excludeVersion;
            _comparisonType = comparisonType;

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    _comparer = StringComparer.CurrentCulture;
                    break;
                case StringComparison.CurrentCultureIgnoreCase:
                    _comparer = StringComparer.CurrentCultureIgnoreCase;
                    break;
                case StringComparison.Ordinal:
                    _comparer = StringComparer.Ordinal;
                    break;
                case StringComparison.OrdinalIgnoreCase:
                    _comparer = StringComparer.OrdinalIgnoreCase;
                    break;
                default:
                    throw new ArgumentException("The comparison type is not supported", nameof(comparisonType));
            }
        }

        public bool Equals(IComponent x, IComponent y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(IComponent? obj)
        {
            var num = 0;
            if (obj?.Name != null)
                num ^= _comparer.GetHashCode(obj.Name);
            if (!_excludeVersion && obj?.CurrentVersion != null)
                num ^= obj.CurrentVersion.GetHashCode();
            return num;
        }

        public int Compare(IComponent? x, IComponent? y)
        {
            if (x == y)
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;
            var num = string.Compare(x.Name, y.Name, _comparisonType);
            if (num != 0)
                return num;
            if (!_excludeVersion)
            {
                var version = x.CurrentVersion;
                num = version != null ? version.CompareTo(y.CurrentVersion) : y.CurrentVersion == null ? 0 : -1;
            }
            return num;
        }
    }
}