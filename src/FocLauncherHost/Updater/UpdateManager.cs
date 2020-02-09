using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FocLauncher;
using FocLauncherHost.Properties;
using FocLauncherHost.Updater.MetadataModel;
using FocLauncherHost.Updater.Tasks;
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

            var updateTasks = (await TryGetUpdateTasksAsync(products, cts.Token)).ToList();
            var deleteTasks = await GetRemovableAssembliesAsync(products);
            var tasks = updateTasks.Union(deleteTasks).ToList();

            if (cts.IsCancellationRequested)
                return CancelledInformation(updateInformation);
            if (tasks.Any(x => x is null))
                return ErrorInformation(updateInformation, "Unable to check dependencies if update is available");
            if (!tasks.Any())
                return SuccessInformation(updateInformation, "No updates available");

            var updateResult =  await UpdateAsync(tasks, cts.Token);


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
        public async Task<UpdateResult> UpdateAsync(IReadOnlyCollection<IDependencyUpdateTask> updateTasks, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            if (updateTasks == null || !updateTasks.Any())
                return UpdateResult.Success;

            if (!updateTasks.Any(x => !(x is DummyTask) && !(x is KeepTask)))
                return UpdateResult.Success;

            Logger.Trace("Performing update...");

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            //var updateCoordinator = new UpdateCoordinator();
            //updateCoordinator.Run(cts.Token);

            try
            {
                foreach (var task in updateTasks)
                {
                    task.Run(cts.Token);
                }
            }
            catch (TaskCanceledException)
            {
                return UpdateResult.Cancelled;
            }
            catch (Exception e)
            {
                Logger.Debug(e, $"Updated failed with exception {e.Message}");
                return UpdateResult.Failed;
            }


            // Something went wrong but the user did NOT request cancellation
            if (cts.IsCancellationRequested)
                return UpdateResult.Failed;



            return UpdateResult.Success;
        }

        public async Task<IEnumerable<IDependencyUpdateTask>> GetUpdateTasksAsync(ProductMetadata product, CancellationToken cancellation = default)
        {
            if (product == null)
                throw new NullReferenceException(nameof(product));
            Logger.Trace($"Try getting update tasks from {product}");
            if (!product.Name.Equals(Product.Name, StringComparison.InvariantCultureIgnoreCase)) 
                throw new NotSupportedException("The product to download does not match with the product initialized");

            // TODO: If any result null we cancel since we cannot promise a stable performance in this case
#if DEBUG // For better debugging
            var result = new HashSet<IDependencyUpdateTask>();
            foreach (var dependency in product.Dependencies)
                result.Add(await CheckDependencyAsync(dependency));
            return result;
#else
             return await product.Dependencies.ForeachAsync(dependency => CheckDependencyAsync(dependency, cancellation));
#endif
        }

        public async Task<IEnumerable<IDependencyUpdateTask>> GetUpdateTasksAsync(ProductsMetadata productsMetadata, CancellationToken cancellation = default)
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

        public async Task<IEnumerable<UpdateTask>> GetRemovableAssembliesAsync(ProductsMetadata products)
        {
            var product = GetMatchingProduct(products);
            if (product == null)
                throw new NotSupportedException("No products to update are found");
            return await GetRemovableAssembliesAsync(product);
        }

        public async Task<IEnumerable<UpdateTask>> GetRemovableAssembliesAsync(ProductMetadata product)
        {
            if (product == null)
                throw new NullReferenceException(nameof(product));
            Logger.Trace($"Try getting removable files tasks");
            if (!product.Name.Equals(Product.Name, StringComparison.InvariantCultureIgnoreCase))
                throw new NotSupportedException("The product to download does not match with the product initialized");
            return await GetRemovableAssembliesAsync(Product.AppDataPath, product.Dependencies);
        }

        internal Task<IEnumerable<UpdateTask>> GetRemovableAssembliesAsync(string basePath, IReadOnlyCollection<Dependency> dependencies)
        {
            if (basePath == null || !Directory.Exists(basePath))
                return Task.FromResult(Enumerable.Empty<UpdateTask>());
            var localFiles = new DirectoryInfo(basePath).GetFilesByExtensions(FileDeleteExtensionFilter.ToArray());

            var result = new List<UpdateTask>();
            foreach (var file in localFiles)
            {
                if (FileDeleteIgnoreFilter.Any(x => file.Name.EndsWith(x)))
                    continue;
                if (dependencies.Any(x => file.Name.Equals(x.Name) && x.InstallLocation == InstallLocation.AppData))
                    continue;

                Logger.Info($"File marked to get deleted: {file.FullName}");
                result.Add(new RemoveFileTask(file));
            }

            return Task.FromResult<IEnumerable<UpdateTask>>(result); ;
        }

        internal Task<IDependencyUpdateTask> CheckDependencyAsync(Dependency dependency)
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
                        return Task.FromResult<IDependencyUpdateTask>(new DummyTask());
                    basePath = new DirectoryInfo(processPath).Parent?.FullName;
                    break;
                }
            }

            Logger.Trace($"Dependency base path: {basePath}");
            if (basePath == null)
                return Task.FromResult<IDependencyUpdateTask>(new DummyTask());

            var filePath = Path.Combine(basePath, dependency.Name);
            if (File.Exists(filePath))
            {
                var newVersion = dependency.GetVersion();
                var currentVersion = UpdaterUtilities.GetAssemblyVersion(filePath);

                if (currentVersion == null || newVersion > currentVersion)
                {
                    Logger.Info($"Dependency marked to get updated: {dependency}");
                    return Task.FromResult<IDependencyUpdateTask>(new DependencyDownloadTask(dependency));
                }

                if (newVersion == null || newVersion < currentVersion || dependency.Sha2 == null)
                {
                    Logger.Info($"Dependency marked to keep: {dependency}");
                    return Task.FromResult<IDependencyUpdateTask>(new KeepTask());
                }

                var fileHash = UpdaterUtilities.GetSha2(filePath);
                if (fileHash == null || !fileHash.SequenceEqual(dependency.Sha2))
                {
                    Logger.Info($"Dependency marked to get updated: {dependency}");
                    return Task.FromResult<IDependencyUpdateTask>(new DependencyDownloadTask(dependency));
                }

                Logger.Info($"Dependency marked to keep: {dependency}");
                return Task.FromResult<IDependencyUpdateTask>(new KeepTask());
            }

            Logger.Info($"Dependency marked to get updated: {dependency}");
            return Task.FromResult<IDependencyUpdateTask>(new DependencyDownloadTask(dependency));
        }

        private async Task<IEnumerable<IDependencyUpdateTask>> TryGetUpdateTasksAsync(ProductsMetadata productsMetadata, CancellationToken cancellation = default)
        {
            try
            {
                Logger.Trace("Try getting update tasks");
                return await GetUpdateTasksAsync(productsMetadata, cancellation);
            }
            catch (Exception e)
            {
                Logger.Debug(e, "Getting update tasks failed with exception. Returning null instead.");
                return Enumerable.Empty<UpdateTask>();
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

        private async Task<UpdateResult> TryUpdateAsync(IReadOnlyCollection<UpdateTask> updateTasks, CancellationToken token = default)
        {
            try
            {
                Logger.Trace("Try updating");
                return await UpdateAsync(updateTasks, token);
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


    internal class UpdateCoordinator : IEnumerable<IDependencyUpdateTask>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //public event EventHandler<ActivityEventArgs> Error;

        protected ConcurrentQueue<IDependencyUpdateTask> Activities { get; }

        internal bool IsCancelled { get; private set; }

        public UpdateCoordinator()
        {
            Activities = new ConcurrentQueue<IDependencyUpdateTask>();
        }

        public void Run(CancellationToken token)
        {

        }

        public IEnumerator<IDependencyUpdateTask> GetEnumerator()
        {
            return Activities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Activities.GetEnumerator();
        }
    }


    public enum UpdateTaskType
    {
        Keep,
        Download,
        Remove
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
