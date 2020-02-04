using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FocLauncher;
using FocLauncher.Threading;
using FocLauncherHost.Properties;
using FocLauncherHost.Updater.MetadataModel;
using FocLauncherHost.Utilities;
using NLog;

namespace FocLauncherHost.Updater
{
    internal class UpdateManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
            Logger.Info("Start automatic check and update...");
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            var updateInformation = new UpdateInformation();

            var stream = await GetMetadataStreamAsync(cts.Token);
            if (cts.IsCancellationRequested)
                return CancelledInformation(updateInformation);
            if (stream is null || stream.Length == 0)
                return ErrorInformation(updateInformation,
                    $"Unable to get the update metadata from: {UpdateMetadataLocation}");

            if (!await ValidateStreamAsync(stream))
                return ErrorInformation(updateInformation,
                    "Stream validation for metadata failed. Download corrupted?");

            var products =  await TryGetProductFromStreamAsync(stream);
            if (products is null)
                return ErrorInformation(updateInformation,
                    "Failed to deserialize metadata stream. Incompatible version?");

            var results = (await TryGetUpdateTasksAsync(products, cts.Token))?.ToList();
            if (cts.IsCancellationRequested)
                return CancelledInformation(updateInformation);
            if (results is null || results.Any(x => x is null))
                return ErrorInformation(updateInformation, "Unable to check dependencies if update is available");
            if (!results.Any())
                return SuccessInformation(updateInformation, "No updates available");

            return updateInformation;
        }
        
        public async Task<IEnumerable<DependencyCheckResult>> GetUpdateTasksAsync(ProductMetadata product, CancellationToken cancellation = default)
        {
            if (product == null)
                throw new NullReferenceException(nameof(product));
            Logger.Trace($"Try getting update tasks from {product}");
            if (!product.Name.Equals(Product.Name, StringComparison.InvariantCultureIgnoreCase)) 
                throw new NotSupportedException("The product to download does not match with the product initialized");

            // TODO: If any result null we cancel since we cannot promise a stable performance in this case
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
                {
                    Logger.Trace($"Try getting update metadata stream from local file: {UpdateMetadataLocation.LocalPath}");
                    await StreamUtilities.CopyFileToStreamAsync(UpdateMetadataLocation.LocalPath, metadataStream, cancellation);
                }

                if (UpdateMetadataLocation.Scheme == Uri.UriSchemeHttp ||
                    UpdateMetadataLocation.Scheme == Uri.UriSchemeHttps)
                {
                    Logger.Trace($"Try getting update metadata stream from online file: {UpdateMetadataLocation.AbsolutePath}");
                    throw new NotImplementedException();
                }

                Logger.Info($"Retrieved metadata stream from {UpdateMetadataLocation}");
                return metadataStream;
            }
            catch (TaskCanceledException)
            {
                Logger.Trace($"Getting metadata stream was cancelled");
                return null;
            }
        }

        private Task<DependencyCheckResult?> CheckDependencyAsync(Dependency dependency, CancellationToken cancellation)
        {
            var basePath = string.Empty;
            Logger.Trace($"Check dependency if update required: {dependency}");
            switch (dependency.InstallLocation)
            {
                case InstallLocation.AppData:
                    basePath = LauncherConstants.ApplicationBasePath;
                    break;
                case InstallLocation.Current:
                {
                    var processPath = Process.GetCurrentProcess().MainModule?.FileName;
                    if (processPath == null)
                        return Task.FromResult<DependencyCheckResult>(null);
                    basePath = new DirectoryInfo(processPath).Parent?.FullName;
                    break;
                }
            }

            Logger.Trace($"Dependency base path: {basePath}");
            if (basePath == null)
                return Task.FromResult<DependencyCheckResult>(null);

            var filePath = Path.Combine(basePath, dependency.Name);

            return Task.FromResult<DependencyCheckResult>(null);
        }

        private async Task<IEnumerable<DependencyCheckResult>?> TryGetUpdateTasksAsync(ProductsMetadata productsMetadata, CancellationToken cancellation = default)
        {
            try
            {
                Logger.Trace("Try getting update tasks");
                return await GetUpdateTasksAsync(productsMetadata, cancellation);
            }
            catch (Exception e)
            {
                Logger.Debug(e, "Getting update tasks failed with exception. Returning null instead.");
                return null;
            }
        }

        private static Task<ProductsMetadata> TryGetProductFromStreamAsync(Stream stream)
        {
            try
            {
                Logger.Trace("Try deserializing stream to ProductsMetadata");
                return ProductsMetadata.DeserializeAsync(stream);
            }
            catch (Exception e)
            {
                Logger.Debug(e, "Getting products from stream failed with exception. Returning null instead.");
                return null;
            }
        }

        private static async Task<bool> ValidateStreamAsync(Stream inputStream)
        {
            var schemeStream = Resources.UpdateValidator.ToStream();
            var validator = new XmlValidator(schemeStream);
            return await Task.FromResult(validator.Validate(inputStream));
        }

        private static UpdateInformation SuccessInformation(UpdateInformation updateInformation, string message, bool requiresRestart = false, bool userNotification = false)
        {
            Logger.Debug("Operation was completed sucessfully");
            updateInformation.Result = requiresRestart ? UpdateResult.SuccessRequiresRestart : UpdateResult.Success;
            updateInformation.Message = message;
            updateInformation.RequiresUserNotification = userNotification;
            return updateInformation;
        }

        private static UpdateInformation ErrorInformation(UpdateInformation updateInformation, string errorMessage, bool userNotification = false)
        {
            Logger.Debug($"Operation failed with message: {errorMessage}");
            updateInformation.Result = UpdateResult.Error;
            updateInformation.Message = errorMessage;
            updateInformation.RequiresUserNotification = userNotification;
            return updateInformation;
        }

        private static UpdateInformation CancelledInformation(UpdateInformation updateInformation, bool userNotification = false)
        {
            Logger.Debug("Operation was cancelled by user request");
            updateInformation.Result = UpdateResult.UserCancelled;
            updateInformation.Message = "Operation cancelled by user request";
            updateInformation.RequiresUserNotification = userNotification;
            return updateInformation;
        }
    }

    public class DependencyCheckResult
    {
        public  Dependency Dependency { get; }

        public DependencyAction RequiredAction { get; set; }

        public DependencyCheckResult(Dependency dependency, DependencyAction requiredAction)
        {
            Dependency = dependency;
            RequiredAction = requiredAction;
        }
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

        public override string ToString()
        {
            return $"Result: {Result}, Message: {Message}";
        }
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
