using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FocLauncher.Threading;
using FocLauncherHost.Updater.MetadataModel;
using FocLauncherHost.Utilities;

namespace FocLauncherHost.Updater
{
    internal class UpdateManager
    {
        public Uri UpdateMetadataLocation { get; }

        public IProductInfo Product { get; }

        public UpdateManager(IProductInfo productInfo, string versionMetadataPath)
        {
            if (!Uri.TryCreate(versionMetadataPath, UriKind.Absolute, out var metadataUri))
                throw new UriFormatException();
            UpdateMetadataLocation = metadataUri;
            Product = productInfo;
        }

        public async Task<UpdateInformation> CheckAndPerformUpdateAsync(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            var updateInformation = new UpdateInformation();

            var stream = await GetMetadataStreamAsync(cts.Token);
            if (cts.IsCancellationRequested)
                return CancelledInformation(updateInformation);
            if (stream == null || stream.Length == 0)
                return ErrorInformation(updateInformation, "Unable to get the update metadata stream");

            var products =  await GetProductFromStreamAsync(stream);
            if (products == null)
                return ErrorInformation(updateInformation, "Failed to deserialize the update metadata");

            var results = await TryGetUpdateTasksAsync(products, cts.Token);
            if (cts.IsCancellationRequested)
                return CancelledInformation(updateInformation);

            return updateInformation;
        }
        
        public async Task<IEnumerable<DependencyCheckResult>> GetUpdateTasksAsync(ProductMetadata product, CancellationToken cancellation = default)
        {
            if (product == null)
                throw new NullReferenceException();
            if (!product.Name.Equals(Product.Name, StringComparison.InvariantCultureIgnoreCase)) 
                throw new NotSupportedException("The product to download does not match with the product initialized");

#if DEBUG // For better debugging
            var result = new List<DependencyCheckResult>();
            foreach (var dependency in product.Dependencies)
                result.Add(await CheckDependencyAsync(dependency, cancellation));
            return result;
#else
             return await product.Dependencies.ForeachAsync(dependency => CheckDependencyAsync(dependency, cancellation));
#endif
        }

        public async Task<IEnumerable<DependencyCheckResult>> GetUpdateTasksAsync(ProductsMetadata productsMetadata, CancellationToken cancellation = default)
        {
            if (productsMetadata == null)
                throw new NullReferenceException(nameof(productsMetadata));
            if (productsMetadata.Products == null || !productsMetadata.Products.Any())
                throw new NotSupportedException("No products to update are found");

            var product = productsMetadata.Products.FirstOrDefault(x =>
                x.Name.Equals(Product.Name, StringComparison.InvariantCultureIgnoreCase));
            if (product == null)
                throw new NotSupportedException("No products to update are found");
            return await GetUpdateTasksAsync(product, cancellation);
        }

        public async Task<Stream?> GetMetadataStreamAsync(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            try
            {
                Stream metadataStream = new MemoryStream();
                if (UpdateMetadataLocation.Scheme == Uri.UriSchemeFile)
                    await StreamUtilities.CopyFileToStreamAsync(UpdateMetadataLocation.LocalPath, metadataStream, cancellation);

                if (UpdateMetadataLocation.Scheme == Uri.UriSchemeHttp ||
                    UpdateMetadataLocation.Scheme == Uri.UriSchemeHttps)
                {
                    throw new NotImplementedException();
                }



                return metadataStream;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        private async Task<DependencyCheckResult> CheckDependencyAsync(Dependency dependency, CancellationToken cancellation)
        {
            string basePath;
            return null;
        }

        private async Task<IEnumerable<DependencyCheckResult>?> TryGetUpdateTasksAsync(ProductsMetadata productsMetadata, CancellationToken cancellation = default)
        {
            try
            {
                return await GetUpdateTasksAsync(productsMetadata, cancellation);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static Task<ProductsMetadata> GetProductFromStreamAsync(Stream stream)
        {
            try
            {
                return ProductsMetadata.DeserializeAsync(stream);
            }
            catch
            {
                return null;
            }
        }

        private static UpdateInformation ErrorInformation(UpdateInformation updateInformation, string errorMessage)
        {
            updateInformation.Result = UpdateResult.Error;
            updateInformation.Message = errorMessage;
            return updateInformation;
        }

        private static UpdateInformation CancelledInformation(UpdateInformation updateInformation)
        {
            updateInformation.Result = UpdateResult.UserCancelled;
            updateInformation.Message = "Operation cancelled by user request";
            return updateInformation;
        }
    }

    public class DependencyCheckResult
    {
        public  Dependency Dependency { get; }

        public DependencyAction RequiredAction { get; }
    }

    public enum DependencyAction
    {
        Keep,
        Update,
        Remove
    }


    public class UpdateInformation
    {
        public UpdateResult Result { get; set; }

        public bool RequiresUserNotification { get; set; }

        public string Message { get; set; }
    }

    public enum UpdateResult
    {
        Success,
        SuccessRequiresRestart,
        Error,
        HashMismatch,
        UserCancelled
    }
}
