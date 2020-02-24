using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FocLauncherHost.Updater.Component;
using FocLauncherHost.Updater.Download;
using FocLauncherHost.Updater.FileSystem;
using FocLauncherHost.Updater.Restart;
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

        protected UpdateManager(IProductInfo productInfo, string versionMetadataPath)
        {
            if (!Uri.TryCreate(versionMetadataPath, UriKind.Absolute, out var metadataUri))
                throw new UriFormatException();
            UpdateCatalogLocation = metadataUri;
            Product = productInfo;
        }


        public virtual async Task<UpdateInformation> CheckAndPerformUpdateAsync(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            Logger.Info("Start automatic check and update...");
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            var updateInformation = new UpdateInformation();

            try
            {
                var stream = await GetMetadataStreamAsync(cts.Token);
                cts.Token.ThrowIfCancellationRequested();

                if (stream is null || stream.Length == 0)
                    throw new UpdaterException($"Unable to get the update metadata from: {UpdateCatalogLocation}");
                if (!await ValidateCatalogStreamAsync(stream))
                    throw new UpdaterException("Stream validation for metadata failed. Download corrupted?");

                try
                {
                    var components = await GetCatalogComponentsAsync(stream, cts.Token);
                    _components.AddRange(components);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed processing catalog: {e.Message}");
                    throw;
                }


                await CalculateComponentStatusAsync(cts.Token);
                await CalculateRemovableComponentsAsync();

                cts.Token.ThrowIfCancellationRequested();

                if (!Components.Any() && !RemovableComponents.Any())
                    throw new UpdaterException("Unable to check dependencies if update is available");

                await UpdateAsync(cts.Token);

                if (LockedFilesWatcher.Instance.LockedFiles.Any())
                {
                    var components = FindComponentsFromFiles(LockedFilesWatcher.Instance.LockedFiles).ToList();
                    var p = LockingProcessManager.Create();
                    p.Register(LockedFilesWatcher.Instance.LockedFiles);
                    await HandleRestartRequestAsync(components, p, cts.Token);
                }
                SuccessInformation(updateInformation, "Success");
            }
            catch (OperationCanceledException)
            {
                CancelledInformation(updateInformation);
            }
            catch (UpdaterException e)
            {
                ErrorInformation(updateInformation, e.Message);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed processing catalog: {e.Message}");
                throw;
            }
            finally
            {
                switch (updateInformation.Result)
                {
                    case UpdateResult.Success:
                        BackupManager.Instance.RemoveAllBackups();
                        break;
                    case UpdateResult.Failed:
                    case UpdateResult.Cancelled:
                    case UpdateResult.SuccessRestartRequired:
                        BackupManager.Instance.RestoreAllBackups();
                        break;
                }
            }

            return updateInformation;
        }

        protected virtual Task HandleRestartRequestAsync(ICollection<IComponent> pendingComponents, ILockingProcessManager lockingProcessManager, CancellationToken token)
        {
            throw new RestartDeniedOrFailedException("Handling restart is not implemented");
        }

        public async Task<UpdateResult> UpdateAsync(CancellationToken cancellation)
        {
            var allComponents = Components.Concat(RemovableComponents);
            return await UpdateAsync(allComponents, cancellation);
        }

        protected internal async Task<UpdateResult> UpdateAsync(IEnumerable<IComponent> components,
            CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            Logger.Trace("Performing update...");

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            var operation = new UpdateOperation(Product, components);
            try
            {
                await Task.Run(() =>
                {
                    operation.Schedule();
                    operation.Run(cts.Token);
                }, cts.Token);

                return UpdateResult.Success;
            }
            catch (OperationCanceledException e)
            {
                Logger.Error(e, $"Cancelled update: {e.Message}");
                throw;
            }
            catch (ComponentFailedException e)
            {
                Logger?.Error(e, "Component Failed to update");
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed update: {e.Message}");
                throw;
            }
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

        public async Task<Stream> GetMetadataStreamAsync(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            try
            {
                Stream metadataStream = new MemoryStream();
                await DownloadManager.Instance.DownloadAsync(UpdateCatalogLocation, metadataStream, null, cancellation);
                Logger.Info($"Retrieved metadata stream from {UpdateCatalogLocation}");
                return metadataStream;
            }
            catch (OperationCanceledException)
            {
                Logger.Trace("Getting metadata stream was cancelled");
                throw;
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

                if (newVersion is null)
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

                var hashResult = HashVerifier.VerifyFile(filePath, component.OriginInfo.ValidationContext);
                if (hashResult == ValidationResult.HashMismatch)
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

        protected static void SuccessInformation(UpdateInformation updateInformation, string message, bool requiresRestart = false, bool userNotification = false)
        {
            Logger.Debug("Operation was completed sucessfully");
            updateInformation.Result = requiresRestart ? UpdateResult.SuccessRestartRequired : UpdateResult.Success;
            updateInformation.Message = message;
            updateInformation.RequiresUserNotification = userNotification;
        }

        protected static void ErrorInformation(UpdateInformation updateInformation, string errorMessage, bool userNotification = false)
        {
            Logger.Debug($"Operation failed with message: {errorMessage}");
            updateInformation.Result = UpdateResult.Failed;
            updateInformation.Message = errorMessage;
            updateInformation.RequiresUserNotification = userNotification;
        }

        protected static void CancelledInformation(UpdateInformation updateInformation, bool userNotification = false)
        {
            Logger.Debug("Operation was cancelled by user request");
            updateInformation.Result = UpdateResult.Cancelled;
            updateInformation.Message = "Operation cancelled by user request";
            updateInformation.RequiresUserNotification = userNotification;
        }

        private IEnumerable<IComponent> FindComponentsFromFiles(IEnumerable<string> files)
        {
            return files.Select(FindComponentsFromFile).Where(component => component != null);
        }

        private IComponent? FindComponentsFromFile(string file)
        {
            return Components.Concat(RemovableComponents).FirstOrDefault(x => x.GetFilePath().Equals(file));
        }
    }
}
