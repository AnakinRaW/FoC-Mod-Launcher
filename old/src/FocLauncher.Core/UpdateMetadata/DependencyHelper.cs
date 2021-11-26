﻿using System;

namespace FocLauncher.UpdateMetadata
{
    public static class DependencyHelper
    {
        public static Version? GetVersion(this Dependency dependency)
        {
            if (string.IsNullOrEmpty(dependency.Version))
                return null;
            return !Version.TryParse(dependency.Version, out var version) ? null : version;
        }
    }
}
