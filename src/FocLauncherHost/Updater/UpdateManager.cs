using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FocLauncherHost.Updater.Component;
using NLog;

namespace FocLauncherHost.Updater
{
    public abstract class UpdateManager
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly List<IComponent> _components = new List<IComponent>();
        private readonly List<IComponent> _removableComponents = new List<IComponent>();
        private ReadOnlyCollection<IComponent> _componentsReadOnly;
        private ReadOnlyCollection<IComponent> _removableComponentsReadOnly;

        public Uri UpdateCatalogLocation { get; }

        public IProductInfo Product { get; }
        
        protected virtual IEnumerable<string> FileDeleteIgnoreFilter => new List<string>();

        protected virtual IEnumerable<string> FileDeleteExtensionFilter => new List<string>{".dll", ".exe"};

        public IReadOnlyCollection<IComponent> Components => _componentsReadOnly ??= new ReadOnlyCollection<IComponent>(_components);

        public IReadOnlyCollection<IComponent> RemovableComponents => _removableComponentsReadOnly ??= new ReadOnlyCollection<IComponent>(_removableComponents);

        //protected UpdateManager(IProductInfo productInfo)
        //{
        //    Product = productInfo;
        //}

        protected UpdateManager(IProductInfo productInfo, string versionMetadataPath) // : this(productInfo)
        {
            if (!Uri.TryCreate(versionMetadataPath, UriKind.Absolute, out var metadataUri))
                throw new UriFormatException();
            UpdateCatalogLocation = metadataUri;
            Product = productInfo;
        }


        // TODO: Do not allow reentracne
        public virtual async Task<UpdateInformation> CheckAndPerformUpdateAsync(CancellationToken cancellation)
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
                    $"Unable to get the update metadata from: {UpdateCatalogLocation}");

            
            if (!await ValidateCatalogStreamAsync(stream))
                return ErrorInformation(updateInformation,
                    "Stream validation for metadata failed. Download corrupted?");

            try
            {
                var components = await GetCatalogComponentsAsync(stream, cts.Token);
                _components.AddRange(components);
            }
            catch (OperationCanceledException)
            {
                return CancelledInformation(updateInformation);
            }
            catch (UpdaterException e)
            {
                return ErrorInformation(updateInformation, e.Message);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed processing catalog: {e.Message}");
                throw;
            }


            await CalculateComponentStatusAsync(cts.Token);
            await CalculateRemovableComponentsAsync();

            if (cts.IsCancellationRequested)
                return CancelledInformation(updateInformation);
            if (!Components.Any() && !RemovableComponents.Any())
                return ErrorInformation(updateInformation, "Unable to check dependencies if update is available");


            var updateResult =  await UpdateAsync(cts.Token);

            if (updateResult == UpdateResult.Cancelled || updateResult == UpdateResult.Failed)
            {
                return ErrorInformation(updateInformation, "TODO");
            }

            if (updateResult != UpdateResult.Success)
            {
                return SuccessInformation(updateInformation, "Successfully updated");
            }

            return SuccessInformation(updateInformation, "Success");
        }

        // TODO: Do not allow reentracne
        public async Task<UpdateResult> UpdateAsync(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            Logger.Trace("Performing update...");

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            var allComponents = Components.Concat(RemovableComponents);


            // TODO: Return ?!....
            await Task.Run(() =>
            {
                var operation = new UpdateOperation(Product, allComponents);
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

            if (cts.IsCancellationRequested)
                return UpdateResult.Failed;

            return UpdateResult.Success;
        }

        public async Task CalculateComponentStatusAsync(CancellationToken cancellation = default)
        {
            Logger.Trace("Calculating current component state");
            
            foreach (var component in Components)
            {
                cancellation.ThrowIfCancellationRequested();
                await Task.Yield();
                await CalculateComponentStatusAsync(component);
            }
        }

        public async Task<Stream?> GetMetadataStreamAsync(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            try
            {
                Stream metadataStream = new MemoryStream();
                if (UpdateCatalogLocation.Scheme == Uri.UriSchemeFile)
                {
                    Logger.Trace($"Try getting update metadata stream from local file: {UpdateCatalogLocation.LocalPath}");
                    await UpdaterUtilities.CopyFileToStreamAsync(UpdateCatalogLocation.LocalPath, metadataStream, cancellation);
                }

                if (UpdateCatalogLocation.Scheme == Uri.UriSchemeHttp ||
                    UpdateCatalogLocation.Scheme == Uri.UriSchemeHttps)
                {
                    Logger.Trace($"Try getting update metadata stream from online file: {UpdateCatalogLocation.AbsolutePath}");
                    throw new NotImplementedException();
                }

                Logger.Info($"Retrieved metadata stream from {UpdateCatalogLocation}");
                return metadataStream;
            }
            catch (TaskCanceledException)
            {
                Logger.Trace("Getting metadata stream was cancelled");
                return null;
            }
        }

        public async Task CalculateRemovableComponentsAsync()
        {
            await CalculateRemovableComponentsAsync(Product.AppDataPath);
        }

        internal Task CalculateRemovableComponentsAsync(string basePath)
        {
            if (basePath == null || !Directory.Exists(basePath))
                return Task.FromException(new DirectoryNotFoundException(nameof(basePath)));


            var localFiles = new DirectoryInfo(basePath).GetFilesByExtensions(FileDeleteExtensionFilter.ToArray());

            foreach (var file in localFiles)
            {
                if (FileDeleteIgnoreFilter.Any(x => file.Name.EndsWith(x)))
                    continue;

                if (!FileCanBeDeleted(file))
                    continue;
                
                Logger.Info($"File marked to get deleted: {file.FullName}");

                var component = new Component.Component
                {
                    Name = file.Name,
                    DiskSize = file.Length,
                    CurrentState = CurrentState.Installed,
                    RequiredAction = ComponentAction.Delete,
                    Destination = file.DirectoryName
                };

                _removableComponents.Add(component);
            }

            return Task.CompletedTask;
        }
        
        protected internal Task CalculateComponentStatusAsync(IComponent component)
        {
            Logger.Trace($"Check dependency if update required: {component}");
            
            var destination = component.Destination;
            Logger.Trace($"Dependency base path: {destination}");
            if (string.IsNullOrEmpty(destination))
                return Task.FromException(new InvalidOperationException());

            var filePath = component.GetFilePath();
            if (File.Exists(filePath))
            {
                var currentVersion = GetComponentVersion(component);
                if (currentVersion == null)
                {
                    Logger.Info($"Dependency marked to get updated: {component}");
                    component.CurrentState = CurrentState.None;
                    component.RequiredAction = ComponentAction.Update;
                    return Task.CompletedTask;
                }

                component.CurrentState = CurrentState.Installed;
                component.CurrentVersion = currentVersion;
                component.DiskSize = new FileInfo(filePath).Length;


                if (component.OriginInfo is null)
                    return Task.CompletedTask;

                var newVersion = component.OriginInfo.Version;

                if (newVersion == null)
                {
                    Logger.Info($"Dependency marked to keep: {component}");
                    return Task.CompletedTask;
                }

                if (newVersion != currentVersion)
                {
                    Logger.Info($"Dependency marked to get updated: {component}");
                    component.RequiredAction = ComponentAction.Update;
                    return Task.CompletedTask;
                }

                if (component.OriginInfo.ValidationContext is null)
                {
                    Logger.Info($"Dependency marked to keep: {component}");
                    return Task.CompletedTask;
                }

                var fileHash = UpdaterUtilities.GetFileHash(filePath, component.OriginInfo.ValidationContext.HashType);
                if (!fileHash.SequenceEqual(component.OriginInfo.ValidationContext.Hash))
                {
                    Logger.Info($"Dependency marked to get updated: {component}");
                    component.RequiredAction = ComponentAction.Update;
                    return Task.CompletedTask;
                }

                Logger.Info($"Dependency marked to keep: {component}");
                return Task.CompletedTask;
            }

            Logger.Info($"Dependency marked to get updated: {component}");
            component.RequiredAction = ComponentAction.Update;
            return Task.CompletedTask;
        }

        protected abstract Task<IEnumerable<IComponent>> GetCatalogComponentsAsync(Stream catalogStream, CancellationToken token);

        protected abstract Task<bool> ValidateCatalogStreamAsync(Stream inputStream);

        protected virtual Version? GetComponentVersion(IComponent component)
        {
            try
            {
                return UpdaterUtilities.GetAssemblyVersion(component.GetFilePath());
            }
            catch
            {
                return null;
            }
        }

        protected virtual bool FileCanBeDeleted(FileInfo file)
        {
            return false;
        }

        protected static UpdateInformation SuccessInformation(UpdateInformation updateInformation, string message, bool requiresRestart = false, bool userNotification = false)
        {
            Logger.Debug("Operation was completed sucessfully");
            updateInformation.Result = requiresRestart ? UpdateInformationResult.SuccessRequiresRestart : UpdateInformationResult.Success;
            updateInformation.Message = message;
            updateInformation.RequiresUserNotification = userNotification;
            return updateInformation;
        }

        protected static UpdateInformation ErrorInformation(UpdateInformation updateInformation, string errorMessage, bool userNotification = false)
        {
            Logger.Debug($"Operation failed with message: {errorMessage}");
            updateInformation.Result = UpdateInformationResult.Error;
            updateInformation.Message = errorMessage;
            updateInformation.RequiresUserNotification = userNotification;
            return updateInformation;
        }

        protected static UpdateInformation CancelledInformation(UpdateInformation updateInformation, bool userNotification = false)
        {
            Logger.Debug("Operation was cancelled by user request");
            updateInformation.Result = UpdateInformationResult.UserCancelled;
            updateInformation.Message = "Operation cancelled by user request";
            updateInformation.RequiresUserNotification = userNotification;
            return updateInformation;
        }
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
