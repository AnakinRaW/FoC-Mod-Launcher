using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FocLauncher;
using FocLauncherHost.Properties;
using FocLauncherHost.UpdateCatalog;
using FocLauncherHost.Updater;
using FocLauncherHost.Updater.Component;
using FocLauncherHost.Utilities;

namespace FocLauncherHost
{
    internal class FocLauncherUpdaterManager : UpdateManager
    {
        protected override IEnumerable<string> FileDeleteIgnoreFilter => new List<string> { ".Theme.dll" };

        public FocLauncherUpdaterManager(string versionMetadataPath) : base(FocLauncherProduct.Instance, versionMetadataPath)
        {
        }

        //public FocLauncherUpdaterManager(IProductInfo product) : base(product)
        //{
            
        //}

        protected override bool FileCanBeDeleted(FileInfo file)
        {
            return !Components.Any(x =>
                file.Name.Equals(x.Name) && x.Destination.Equals(LauncherConstants.ApplicationBasePath));
        }

        protected override async Task<IEnumerable<IComponent>> GetCatalogComponentsAsync(Stream catalogStream, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var products = await TryGetProductFromStreamAsync(catalogStream);
            if (products is null)
                throw new UpdaterException("Failed to deserialize metadata stream. Incompatible version?");

            var product = GetMatchingProduct(products);
            if (product is null)
                throw new UpdaterException("No products to update are found");

            var result = new HashSet<IComponent>(ComponentIdentityComparer.Default);
            foreach (var component in product.Dependencies.Select(DependencyToComponent).Where(component => component != null))
                result.Add(component);
            return result;
        }

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

                ValidationContext validationContext = null;
                if (hash != null)
                    validationContext = new ValidationContext { Hash = hash, HashType = HashType.Sha2 };
                var originInfo = new OriginInfo(new Uri(dependency.Origin, UriKind.Absolute), newVersion, validationContext);
                component.OriginInfo = originInfo;
            }

            return component;
        }

        protected override async Task<bool> ValidateCatalogStreamAsync(Stream inputStream)
        {
            var schemeStream = Resources.UpdateValidator.ToStream();
            var validator = new XmlValidator(schemeStream);
            return await Task.FromResult(validator.Validate(inputStream));
        }

        private static string GetRealDependencyDestination(Dependency dependency)
        {
            var destination = Environment.ExpandEnvironmentVariables(dependency.Destination);
            if (!Uri.TryCreate(destination, UriKind.Absolute, out var uri))
                throw new InvalidOperationException($"No absolute dependency destination: {destination}");
            return uri.LocalPath;
        }

        private static Task<Catalogs> TryGetProductFromStreamAsync(Stream stream)
        {
            try
            {
                Logger.Trace("Try deserializing stream to Catalogs");
                return Catalogs.DeserializeAsync(stream);
            }
            catch (Exception e)
            {
                Logger.Debug(e, "Getting catalogs from stream failed with exception. Returning null instead.");
                return Task.FromResult<Catalogs>(null);
            }
        }

        private ProductCatalog? GetMatchingProduct(Catalogs products)
        {
            if (products == null)
                throw new NullReferenceException(nameof(products));
            if (products.Products == null || !products.Products.Any())
                throw new NotSupportedException("No products to update are found");

            return products.Products.FirstOrDefault(x => x.Name.Equals(Product.Name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}