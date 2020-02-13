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
            SetUpdateConfiguration();
        }

        //public FocLauncherUpdaterManager(IProductInfo product) : base(product)
        //{
            
        //}

        private static void SetUpdateConfiguration()
        {
            UpdateConfiguration.Instance.BackupPolicy = BackupPolicy.Required;
            UpdateConfiguration.Instance.BackupPath = LauncherConstants.ApplicationBasePath;
        }

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

            var product = products.GetMatchingProduct(Product);
            if (product is null)
                throw new UpdaterException("No products to update are found");

            var result = new HashSet<IComponent>(ComponentIdentityComparer.Default);
            foreach (var component in product.Dependencies.Select(DependencyHelper.DependencyToComponent).Where(component => component != null))
                result.Add(component);
            return result;
        }
        
        protected override async Task<bool> ValidateCatalogStreamAsync(Stream inputStream)
        {
            var schemeStream = Resources.UpdateValidator.ToStream();
            var validator = new XmlValidator(schemeStream);
            return await Task.FromResult(validator.Validate(inputStream));
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
    }
}