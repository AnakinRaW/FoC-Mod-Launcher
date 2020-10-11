using System;

namespace FocLauncher.Settings
{
    public readonly struct LanguageFallbackData : IEquatable<LanguageFallbackData>
    {
        public LanguageFallback Option { get; }

        public string DisplayName { get; }

        public string Tooltip { get; }

        public LanguageFallbackData(LanguageFallback option, string name, string tooltip)
        {
            Option = option;
            DisplayName = name;
            Tooltip = tooltip;
        }

        public bool Equals(LanguageFallbackData other)
        {
            return Option == other.Option;
        }

        public override bool Equals(object? obj)
        {
            return obj is LanguageFallbackData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Option;
        }
    }
}