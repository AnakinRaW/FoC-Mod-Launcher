using System;
using FocLauncherHost.Updater.Component;

namespace FocLauncherHost.Update.UpdateCatalog
{
    internal static class DependencyHelper
    {
        internal static IComponent? DependencyToComponent(Dependency dependency)
        {
            if (string.IsNullOrEmpty(dependency.Name) || string.IsNullOrEmpty(dependency.Destination))
                return null;
            var component = new Component
            {
                Name = dependency.Name,
                Destination = GetRealDependencyDestination(dependency)
            };

            if (!string.IsNullOrEmpty(dependency.Origin))
            {
                var newVersion = dependency.GetVersion();
                var hash = dependency.Sha2;
                var size = dependency.Size;

                ValidationContext validationContext = null;
                if (hash != null)
                    validationContext = new ValidationContext { Hash = hash, HashType = HashType.Sha2 };
                var originInfo = new OriginInfo(new Uri(dependency.Origin, UriKind.Absolute), newVersion, size, validationContext);
                component.OriginInfo = originInfo;
            }

            return component;
        }

        internal static Version? GetVersion(this Dependency dependency)
        {
            if (string.IsNullOrEmpty(dependency.Version))
                return null;
            return !Version.TryParse(dependency.Version, out var version) ? null : version;
        }

        private static string GetRealDependencyDestination(Dependency dependency)
        {
            var destination = Environment.ExpandEnvironmentVariables(dependency.Destination);
            if (!Uri.TryCreate(destination, UriKind.Absolute, out var uri))
                throw new InvalidOperationException($"No absolute dependency destination: {destination}");
            return uri.LocalPath;
        }
    }
}
