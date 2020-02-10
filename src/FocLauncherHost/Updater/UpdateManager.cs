using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FocLauncher;
using FocLauncherHost.Properties;
using FocLauncherHost.Updater.MetadataModel;
using FocLauncherHost.Utilities;
using NLog;

namespace FocLauncherHost.Updater
{
    internal class UpdateManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly object _syncObject = new object();

        public Uri UpdateMetadataLocation { get; }

        public IProductInfo Product { get; }
        
        protected virtual IEnumerable<string> FileDeleteIgnoreFilter => new List<string>{".Theme.dll"};

        protected virtual IEnumerable<string> FileDeleteExtensionFilter => new List<string>{".dll", ".exe"};


        public UpdateManager(IProductInfo productInfo, string versionMetadataPath)
        {
            if (!Uri.TryCreate(versionMetadataPath, UriKind.Absolute, out var metadataUri))
                throw new UriFormatException();
            UpdateMetadataLocation = metadataUri;
            Product = productInfo;
        }

        // TODO: Do not allow reentracne
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

            var dependencies = (await TryGetUpdateDependenciesAsync(products, cts.Token)).ToList();
            var removableDependencies = await GetRemovableAssembliesAsync(products);
            var allDependencies = dependencies.Union(removableDependencies).ToList();

            if (cts.IsCancellationRequested)
                return CancelledInformation(updateInformation);
            if (allDependencies.Any(x => x is null))
                return ErrorInformation(updateInformation, "Unable to check dependencies if update is available");
            //if (!tasks.Any())
            //    return SuccessInformation(updateInformation, "No updates available");

            var updateResult =  await UpdateAsync(allDependencies, cts.Token);


            if (updateResult == UpdateResult.Cancelled || updateResult == UpdateResult.Failed)
            {
                // TODO: Restore and return

                return ErrorInformation(updateInformation, "TODO");
            }

            if (updateResult != UpdateResult.Success)
            {
                // TODO: Cleanup
                return SuccessInformation(updateInformation, "Successfully updated");
            }

            // TODO Start external updater, do clean up or restore there
            return SuccessInformation(updateInformation, "Success");
        }

        // TODO: Do not allow reentracne
        public async Task<UpdateResult> UpdateAsync(IReadOnlyCollection<IDependency> updateTasks, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            //if (updateTasks == null || !updateTasks.Any())
            //    return UpdateResult.Success;

            //if (!updateTasks.Any(x => !(x is DummyTask) && !(x is KeepTask)))
            //    return UpdateResult.Success;

            Logger.Trace("Performing update...");

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);


            await Task.Run(() =>
            {
                var operation = new UpdateOperation(Product, updateTasks);
                operation.Schedule();
                try
                {
                    operation.Run(cts.Token);
                }
                catch (OperationCanceledException e)
                {
                    Logger.Error(e, $"Cancelled update: {e.Message}");
                    throw;
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Failed update: {e.Message}");
                    throw;
                }
            }, cts.Token);


            //var updateCoordinator = new UpdateCoordinator();
            //updateCoordinator.Run(cts.Token);



            // Something went wrong but the user did NOT request cancellation
            if (cts.IsCancellationRequested)
                return UpdateResult.Failed;

            return UpdateResult.Success;
        }

        public async Task<ICollection<IDependency>> GetUpdateDependenciesAsync(ProductMetadata product, CancellationToken cancellation = default)
        {
            if (product == null)
                throw new NullReferenceException(nameof(product));
            Logger.Trace($"Try getting update tasks from {product}");
            if (!product.Name.Equals(Product.Name, StringComparison.InvariantCultureIgnoreCase)) 
                throw new NotSupportedException("The product to download does not match with the product initialized");

            var result = new HashSet<IDependency>();
            foreach (var dependency in product.Dependencies)
            {
                cancellation.ThrowIfCancellationRequested();
                await Task.Yield();
                await CheckDependencyAsync(dependency);
                result.Add(dependency);
            }
            return result;
        }

        public async Task<ICollection<IDependency>> GetUpdateDependenciesAsync(ProductsMetadata productsMetadata, CancellationToken cancellation = default)
        {
            var product = GetMatchingProduct(productsMetadata);
            if (product == null)
                throw new NotSupportedException("No products to update are found");
            return await GetUpdateDependenciesAsync(product, cancellation);
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

        public async Task<ICollection<IDependency>> GetRemovableAssembliesAsync(ProductsMetadata products)
        {
            var product = GetMatchingProduct(products);
            if (product == null)
                throw new NotSupportedException("No products to update are found");
            return await GetRemovableAssembliesAsync(product);
        }

        public async Task<ICollection<IDependency>> GetRemovableAssembliesAsync(ProductMetadata product)
        {
            if (product == null)
                throw new NullReferenceException(nameof(product));
            Logger.Trace($"Try getting removable files tasks");
            if (!product.Name.Equals(Product.Name, StringComparison.InvariantCultureIgnoreCase))
                throw new NotSupportedException("The product to download does not match with the product initialized");
            return await GetRemovableAssembliesAsync(Product.AppDataPath, product.Dependencies);
        }

        internal Task<ICollection<IDependency>> GetRemovableAssembliesAsync(string basePath, IReadOnlyCollection<IDependency> dependencies)
        {
            if (basePath == null || !Directory.Exists(basePath))
                return Task.FromResult<ICollection<IDependency>>(new List<IDependency>());
            var localFiles = new DirectoryInfo(basePath).GetFilesByExtensions(FileDeleteExtensionFilter.ToArray());

            var result = new List<IDependency>();
            foreach (var file in localFiles)
            {
                if (FileDeleteIgnoreFilter.Any(x => file.Name.EndsWith(x)))
                    continue;
                if (dependencies.Any(x => file.Name.Equals(x.Name) && x.InstallLocation == InstallLocation.AppData))
                    continue;

                Logger.Info($"File marked to get deleted: {file.FullName}");

                var dependency = new Dependency
                {
                    Name = file.Name,
                    Version = UpdaterUtilities.GetAssemblyVersion(file.FullName).ToString(),
                    CurrentState = CurrentDependencyState.Installed,
                    RequiredAction = DependencyAction.Delete,
                    InstallLocation = InstallLocation.AppData
                };
                
                result.Add(dependency);
            }

            return Task.FromResult<ICollection<IDependency>>(result);
        }

        internal Task CheckDependencyAsync(IDependency dependency)
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
                        return Task.FromException(new InvalidOperationException());
                    basePath = new DirectoryInfo(processPath).Parent?.FullName;
                    break;
                }
            }

            Logger.Trace($"Dependency base path: {basePath}");
            if (basePath == null)
                return Task.FromException(new InvalidOperationException());

            var filePath = Path.Combine(basePath, dependency.Name);
            if (File.Exists(filePath))
            {
                var newVersion = dependency.GetVersion();
                var currentVersion = UpdaterUtilities.GetAssemblyVersion(filePath);

                if (newVersion == null)
                {
                    Logger.Info($"Dependency marked to keep: {dependency}");
                    dependency.CurrentState = currentVersion == null ? CurrentDependencyState.None : CurrentDependencyState.Installed;
                    return Task.CompletedTask;
                }

                if (currentVersion == null || newVersion != currentVersion)
                {
                    Logger.Info($"Dependency marked to get updated: {dependency}");
                    dependency.CurrentState = currentVersion == null
                        ? CurrentDependencyState.None
                        : CurrentDependencyState.Installed;
                    dependency.RequiredAction = DependencyAction.Update;
                    return Task.CompletedTask;
                }

                if (dependency.Sha2 == null)
                {
                    Logger.Info($"Dependency marked to keep: {dependency}");
                    return Task.CompletedTask;
                }

                var fileHash = UpdaterUtilities.GetSha2(filePath);
                if (fileHash == null || !fileHash.SequenceEqual(dependency.Sha2))
                {
                    Logger.Info($"Dependency marked to get updated: {dependency}");
                    dependency.CurrentState = CurrentDependencyState.Installed;
                    dependency.RequiredAction = DependencyAction.Update;
                    return Task.CompletedTask;
                }

                Logger.Info($"Dependency marked to keep: {dependency}");
                return Task.CompletedTask;
            }

            Logger.Info($"Dependency marked to get updated: {dependency}");
            dependency.RequiredAction = DependencyAction.Update;
            return Task.CompletedTask;
        }

        private async Task<ICollection<IDependency>> TryGetUpdateDependenciesAsync(ProductsMetadata productsMetadata, CancellationToken cancellation = default)
        {
            try
            {
                Logger.Trace("Try getting update tasks");
                return await GetUpdateDependenciesAsync(productsMetadata, cancellation);
            }
            catch (OperationCanceledException e)
            {
                Logger.Trace(e, "Getting dependencies was cancelled. Returning empty instead.");
                return new List<IDependency>();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Getting dependencies failed with exception. Returning empty instead.");
                return new List<IDependency>();
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
                return Task.FromResult<ProductsMetadata>(null);
            }
        }

        private async Task<UpdateResult> TryUpdateAsync(IReadOnlyCollection<Dependency> dependencies, CancellationToken token = default)
        {
            try
            {
                Logger.Trace("Try updating");
                return await UpdateAsync(dependencies, token);
            }
            catch (TaskCanceledException)
            {
                return UpdateResult.Cancelled;
            }
            catch (Exception)
            {
                return UpdateResult.Failed;
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
            updateInformation.Result = requiresRestart ? UpdateInformationResult.SuccessRequiresRestart : UpdateInformationResult.Success;
            updateInformation.Message = message;
            updateInformation.RequiresUserNotification = userNotification;
            return updateInformation;
        }

        private static UpdateInformation ErrorInformation(UpdateInformation updateInformation, string errorMessage, bool userNotification = false)
        {
            Logger.Debug($"Operation failed with message: {errorMessage}");
            updateInformation.Result = UpdateInformationResult.Error;
            updateInformation.Message = errorMessage;
            updateInformation.RequiresUserNotification = userNotification;
            return updateInformation;
        }

        private static UpdateInformation CancelledInformation(UpdateInformation updateInformation, bool userNotification = false)
        {
            Logger.Debug("Operation was cancelled by user request");
            updateInformation.Result = UpdateInformationResult.UserCancelled;
            updateInformation.Message = "Operation cancelled by user request";
            updateInformation.RequiresUserNotification = userNotification;
            return updateInformation;
        }

        private ProductMetadata? GetMatchingProduct(ProductsMetadata products)
        {
            if(products == null)
            throw new NullReferenceException(nameof(products));
            if (products.Products == null || !products.Products.Any())
                throw new NotSupportedException("No products to update are found");

            return products.Products.FirstOrDefault(x => x.Name.Equals(Product.Name, StringComparison.InvariantCultureIgnoreCase));
        }
    }


    public enum UpdateTaskStatus
    {
        NotStarted,
        CompletedSuccess
    }


    public class UpdateInformation
    {
        public UpdateInformationResult Result { get; set; }

        public bool RequiresUserNotification { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return $"Result: {Result}, Message: {Message}";
        }
    }

    public enum UpdateResult
    {
        Failed,
        Success,
        SuccessPending,
        Cancelled
    }

    public enum UpdateInformationResult
    {
        Success,
        SuccessRequiresRestart,
        Error,
        UserCancelled
    }
}
