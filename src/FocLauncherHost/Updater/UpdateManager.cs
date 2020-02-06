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

        private List<DependencyUpdateResult> UpdateResults { get; } = new List<DependencyUpdateResult>();



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

            var updateTasks = (await TryGetUpdateTasksAsync(products, cts.Token)).ToList();
            var deleteTasks = await GetRemovableAssembliesAsync(products);
            var tasks = updateTasks.Union(deleteTasks).ToList();

            var actionOnlyTasks = tasks.Where(x => x.RequiredAction != DependencyAction.Keep).ToList();

            if (cts.IsCancellationRequested)
                return CancelledInformation(updateInformation);
            if (actionOnlyTasks.Any(x => x is null))
                return ErrorInformation(updateInformation, "Unable to check dependencies if update is available");
            if (!actionOnlyTasks.Any())
                return SuccessInformation(updateInformation, "No updates available");

            var updateResult =  await TryUpdateAsync(actionOnlyTasks, cts.Token);

            if (updateResult == UpdateActionResult.Cancelled || updateResult == UpdateActionResult.Failed)
            {
                // TODO: Clean up
            }


            return SuccessInformation(updateInformation, "Success");
        }

        // TODO: Do not allow reentracne
        public async Task<UpdateActionResult> UpdateAsync(IReadOnlyCollection<DependencyCheckResult> updateTasks, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            if (updateTasks == null || !updateTasks.Any())
                return UpdateActionResult.Success;

            var actionOnlyTasks = updateTasks.Where(x => x.RequiredAction != DependencyAction.Keep).ToList();
            if (!actionOnlyTasks.Any())
                return UpdateActionResult.Success;

            Logger.Trace("Performing update...");

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            var pendingFlag = false;
            foreach (var task in updateTasks)
            {
                if (cts.IsCancellationRequested)
                    break;

                DependencyUpdateResult updateResult;

                if (task.RequiredAction == DependencyAction.Keep)
                    updateResult = new DependencyKeepResult(task.Dependency);

                if (task.RequiredAction == DependencyAction.Remove)
                {
                    updateResult = UpdaterUtilities.TryRemoveDependency(task, cts);
                }
                else
                {
                    // TODO
                    updateResult = new DependencyDownloadResult(task.Dependency, UpdateActionResult.Success);
                }

                Logger.Trace(
                    $"Performed update task {updateResult.PerformedAction} on dependency '{updateResult.Dependency.Name}' " +
                    $"with result: {updateResult.UpdateResult}");

                if (updateResult.UpdateResult == UpdateActionResult.Pending)
                    pendingFlag = true;
                AddResult(updateResult);
            }

            // Something went wrong but the user did NOT request cancellation
            if (cts.IsCancellationRequested)
                return UpdateActionResult.Failed;
            return pendingFlag ? UpdateActionResult.Pending : UpdateActionResult.Success;
        }

        private void AddResult(DependencyUpdateResult result)
        {
            lock (_syncObject)
            {
                UpdateResults.Add(result);
            }
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
            var result = new HashSet<DependencyCheckResult>();
            foreach (var dependency in product.Dependencies)
                result.Add(await CheckDependencyAsync(dependency));
            return result;
#else
             return await product.Dependencies.ForeachAsync(dependency => CheckDependencyAsync(dependency, cancellation));
#endif
        }

        public async Task<IEnumerable<DependencyCheckResult>> GetUpdateTasksAsync(ProductsMetadata productsMetadata, CancellationToken cancellation = default)
        {
            var product = GetMatchingProduct(productsMetadata);
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

        public async Task<IEnumerable<DependencyCheckResult>> GetRemovableAssembliesAsync(ProductsMetadata products)
        {
            var product = GetMatchingProduct(products);
            if (product == null)
                throw new NotSupportedException("No products to update are found");
            return await GetRemovableAssembliesAsync(product);
        }

        public async Task<IEnumerable<DependencyCheckResult>> GetRemovableAssembliesAsync(ProductMetadata product)
        {
            if (product == null)
                throw new NullReferenceException(nameof(product));
            Logger.Trace($"Try getting removable files tasks");
            if (!product.Name.Equals(Product.Name, StringComparison.InvariantCultureIgnoreCase))
                throw new NotSupportedException("The product to download does not match with the product initialized");
            return await GetRemovableAssembliesAsync(Product.AppDataPath, product.Dependencies);
        }

        internal Task<IEnumerable<DependencyCheckResult>> GetRemovableAssembliesAsync(string basePath, IReadOnlyCollection<Dependency> dependencies)
        {
            if (basePath == null || !Directory.Exists(basePath))
                return Task.FromResult(Enumerable.Empty<DependencyCheckResult>());
            var localFiles = new DirectoryInfo(basePath).GetFilesByExtensions(FileDeleteExtensionFilter.ToArray());

            var result = new List<DependencyCheckResult>();
            foreach (var file in localFiles)
            {
                if (FileDeleteIgnoreFilter.Any(x => file.Name.EndsWith(x)))
                    continue;
                if (dependencies.Any(x => file.Name.Equals(x.Name) && x.InstallLocation == InstallLocation.AppData))
                    continue;

                Logger.Info($"File marked to get deleted: {file.FullName}");
                var dependency = new Dependency {Name = file.Name, InstallLocation = InstallLocation.AppData};
                result.Add(new DependencyCheckResult(dependency, DependencyAction.Remove));
            }

            return Task.FromResult<IEnumerable<DependencyCheckResult>>(result); ;
        }

        internal Task<DependencyCheckResult?> CheckDependencyAsync(Dependency dependency)
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
            if (File.Exists(filePath))
            {
                var newVersion = dependency.GetVersion();
                var currentVersion = UpdaterUtilities.GetAssemblyVersion(filePath);

                if (currentVersion == null || newVersion > currentVersion)
                {
                    Logger.Info($"Dependency marked to get updated: {dependency}");
                    return Task.FromResult(new DependencyCheckResult(dependency, DependencyAction.Update));
                }

                if (newVersion == null || newVersion < currentVersion || dependency.Sha2 == null)
                {
                    Logger.Info($"Dependency marked to keep: {dependency}");
                    return Task.FromResult(new DependencyCheckResult(dependency, DependencyAction.Keep));
                }

                var fileHash = UpdaterUtilities.GetSha2(filePath);
                if (fileHash == null || !fileHash.SequenceEqual(dependency.Sha2))
                {
                    Logger.Info($"Dependency marked to get updated: {dependency}");
                    return Task.FromResult(new DependencyCheckResult(dependency, DependencyAction.Update));
                }

                Logger.Info($"Dependency marked to keep: {dependency}");
                return Task.FromResult(new DependencyCheckResult(dependency, DependencyAction.Keep));
            }

            Logger.Info($"Dependency marked to get updated: {dependency}");
            return Task.FromResult(new DependencyCheckResult(dependency, DependencyAction.Update));
        }

        private async Task<IEnumerable<DependencyCheckResult>> TryGetUpdateTasksAsync(ProductsMetadata productsMetadata, CancellationToken cancellation = default)
        {
            try
            {
                Logger.Trace("Try getting update tasks");
                return await GetUpdateTasksAsync(productsMetadata, cancellation);
            }
            catch (Exception e)
            {
                Logger.Debug(e, "Getting update tasks failed with exception. Returning null instead.");
                return Enumerable.Empty<DependencyCheckResult>();
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

        private async Task<UpdateActionResult> TryUpdateAsync(IReadOnlyCollection<DependencyCheckResult> updateTasks, CancellationToken token = default)
        {
            try
            {
                Logger.Trace("Try updating");
                return await UpdateAsync(updateTasks, token);
            }
            catch (TaskCanceledException)
            {
                return UpdateActionResult.Cancelled;
            }
            catch (Exception)
            {
                return UpdateActionResult.Failed;
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

    public class DependencyCheckResult
    {
        public  Dependency Dependency { get; }

        public DependencyAction RequiredAction { get; set; }

        public DependencyCheckResult(Dependency dependency, DependencyAction requiredAction)
        {
            Dependency = dependency;
            RequiredAction = requiredAction;
        }

        protected bool Equals(DependencyCheckResult other)
        {
            return Dependency.Equals(other.Dependency);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) 
                return false;
            if (ReferenceEquals(this, obj)) 
                return true;
            return obj.GetType() == GetType() && Equals((DependencyCheckResult)obj);
        }

        public override int GetHashCode()
        {
            return Dependency.GetHashCode();
        }
    }

    public abstract class DependencyUpdateResult
    {
        public Dependency Dependency { get; }

        public UpdateActionResult UpdateResult { get; }

        public DependencyAction PerformedAction { get; }

        public DependencyUpdateResult(Dependency dependency, DependencyAction performedAction, UpdateActionResult updateResult)
        {
            Dependency = dependency;
            PerformedAction = performedAction;
            UpdateResult = updateResult;
        }
    }

    public sealed class DependencyKeepResult : DependencyUpdateResult
    {
        public DependencyKeepResult(Dependency dependency) : base(dependency, DependencyAction.Keep, UpdateActionResult.Success)
        {
        }
    }

    public sealed class DependencyRemoveResult : DependencyUpdateResult
    {
        public string RecoveryFile { get; }

        public DependencyRemoveResult(Dependency dependency, UpdateActionResult result) : base(dependency, DependencyAction.Remove, result)
        {
        }
    }

    public sealed class DependencyDownloadResult : DependencyUpdateResult
    {
        public string DownloadedFile { get; }

        public DependencyDownloadResult(Dependency dependency, UpdateActionResult result) : base(dependency, DependencyAction.Update, result)
        {
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
        public UpdateInformationResult Result { get; set; }

        public bool RequiresUserNotification { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return $"Result: {Result}, Message: {Message}";
        }
    }

    public enum UpdateActionResult
    {
        Failed,
        Success,
        Pending,
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
