﻿using System;
using System.Collections.Generic;

namespace FocLauncherHost.Updater
{
    public interface IComponent : IEquatable<IComponent>
    {
        string Destination { get; set; }

        string Name { get; set; }
        
        ComponentAction RequiredAction { get; set; }

        CurrentState CurrentState { get; set; }

        Version? CurrentVersion { get; set; }

        OriginInfo? OriginInfo { get; set; }
    }

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

        public int GetHashCode(IComponent obj)
        {
            var num = 0;
            if (obj.Name != null)
                num ^= _comparer.GetHashCode(obj.Name);
            if (!_excludeVersion && obj.CurrentVersion != null)
                num ^= obj.CurrentVersion.GetHashCode();
            return num;
        }

        public int Compare(IComponent x, IComponent y)
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
                num = (object) version != null ? version.CompareTo(y.CurrentVersion) : y.CurrentVersion == null ? 0 : -1;
            }
            return num;
        }
    }


    public class Component : IComponent
    {
        public string Destination { get; set; }

        public string Name { get; set; }

        public ComponentAction RequiredAction { get; set; }

        public CurrentState CurrentState { get; set; }

        public Version? CurrentVersion { get; set; }

        public OriginInfo? OriginInfo { get; set; }
        
        public override string ToString()
        {
            return !string.IsNullOrEmpty(Name) ? $"{Name},destination='{Destination}'" : base.ToString();
        }

        public bool Equals(IComponent other)
        {
            return ComponentIdentityComparer.Default.Equals(this, other);
        }
    }

    public enum ComponentAction
    {
        Keep,
        Update,
        Delete
    }

    public enum CurrentState
    {
        None,
        Downloaded,
        Installed,
        Removed,
    }

    public class OriginInfo
    {
        public Uri Origin { get; }

        public Version? Version { get; }

        public ValidationContext? ValidationContext { get; }

        public OriginInfo(Uri origin, Version version = null, ValidationContext validationContext = null)
        {
            Origin = origin ?? throw new ArgumentNullException(nameof(origin));
            Version = version;
            ValidationContext = validationContext;
        }
    }

    public class ValidationContext
    {
        public byte[] Hash { get; set; }

        public HashType HashType { get; set; }
    }

    public enum HashType
    {
        None,
        MD5,
        Sha1,
        Sha2,
        Sha3
    }
}
