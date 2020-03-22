using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FocLauncher;
using FocLauncher.Shared;
using FocLauncherHost.Properties;
using FocLauncherHost.Update.UpdateCatalog;
using FocLauncherHost.Utilities;
using Newtonsoft.Json;
using TaskBasedUpdater;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.Restart;

namespace FocLauncherHost
{
    internal class FocLauncherUpdaterManager : UpdateManager
    {
        protected override IEnumerable<string> FileDeleteIgnoreFilter => new List<string> { ".Theme.dll" };

        public FocLauncherUpdaterManager(string versionMetadataPath) : base(FocLauncherProduct.Instance, versionMetadataPath)
        {
            SetUpdateConfiguration();
        }

        private static void SetUpdateConfiguration()
        {
            UpdateConfiguration.Instance.BackupPolicy = BackupPolicy.Required;
            UpdateConfiguration.Instance.DownloadRetryCount = 1;
            UpdateConfiguration.Instance.BackupPath = Path.Combine(LauncherConstants.ApplicationBasePath, "Backups");
            UpdateConfiguration.Instance.DownloadRetryDelay = 500;
            UpdateConfiguration.Instance.SupportsRestart = true;
            UpdateConfiguration.Instance.ExternalUpdaterPath = LauncherConstants.UpdaterPath;
            UpdateConfiguration.Instance.ExternalElevatorPath = LauncherConstants.ElevatorPath;
            UpdateConfiguration.Instance.RequiredElevationCancelsUpdate = true;
            UpdateConfiguration.Instance.AlternativeDownloadPath = Path.Combine(LauncherConstants.ApplicationBasePath, "Downloads");
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

        protected override IRestartOptions CreateRestartOptions(IReadOnlyCollection<IComponent>? components = null)
        {
            var options = new LauncherRestartOptions
            {
                Pid = Process.GetCurrentProcess().Id,
                ExecutablePath = Environment.GetCommandLineArgs()[0],
                Update = components != null && components.Any()
            };

            if (options.Update && components != null)
            {
                var output = JsonConvert.SerializeObject(GetUpdaterItems(components));
                options.Payload = Base64Encode(output);
            }

            
            var args = options.Unparse();
            Logger.Debug($"Created restart options: {args}");
            return options;
        }
        
        protected override bool PermitElevationRequest()
        {
            return LauncherRestartManager.ShowElevateDialog();
        }


        protected override async Task<PendingHandledResult> HandleLockedComponentsCoreAsync(
            ICollection<IComponent> pendingComponents, ILockingProcessManager lockingProcessManager,
            bool ignoreSelfLocked, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!pendingComponents.Any())
                return new PendingHandledResult(HandlePendingComponentsStatus.Handled);

            Logger.Trace("Hanlde restart request due to locked files");

            var processes = lockingProcessManager.GetProcesses().ToList();
            var isSelfLocking = lockingProcessManager.ProcessesContainsSelf();

            if (!isSelfLocking && processes.Any(x => x.ApplicationType == ApplicationType.Critical))
                return new PendingHandledResult(HandlePendingComponentsStatus.Declined,
                    "Files are locked by a system process that cannot be terminated. Please restart the system");

            using var lockingProcessManagerWithoutSelf = CreateFromProcessesWithoutSelf(processes);

            if (!isSelfLocking || ignoreSelfLocked)
            {
                var restartRequestResult =
                    LauncherRestartManager.ShowProcessKillDialog(lockingProcessManagerWithoutSelf, token);
                Logger.Trace($"Kill locking processes: {restartRequestResult}, Launcher needs restart: {false}");
                if (!restartRequestResult)
                    return new PendingHandledResult(HandlePendingComponentsStatus.Declined,
                        "Update aborted because locked files have not been released.");

                lockingProcessManagerWithoutSelf.Shutdown();
                LockedFilesWatcher.Instance.LockedFiles.Clear();
                await UpdateAsync(pendingComponents, token).ConfigureAwait(false);

                return LockedFilesWatcher.Instance.LockedFiles.Any()
                    ? new PendingHandledResult(HandlePendingComponentsStatus.HandledButStillPending,
                        "Update failed because there are still locked files which have not been released.")
                    : new PendingHandledResult(HandlePendingComponentsStatus.Handled);
            }

            if (!UpdateConfiguration.Instance.SupportsRestart)
                return new PendingHandledResult(HandlePendingComponentsStatus.Declined,
                    "Update requires a self-update which is not supported for this update configuration.");

            var result = LauncherRestartManager.ShowSelfKillDialog(lockingProcessManager, token);
            Logger.Trace($"Kill locking processes: {result}, Launcher needs restart: {true}");
            if (!result)
                return new PendingHandledResult(HandlePendingComponentsStatus.Declined,
                    "Update aborted because locked files have not been released.");

            lockingProcessManagerWithoutSelf.Shutdown();
            return new PendingHandledResult(HandlePendingComponentsStatus.Restart);
        }


        private ILockingProcessManager CreateFromProcessesWithoutSelf(IEnumerable<ILockingProcessInfo> processes)
        {
            var processesWithoutSelf = WithoutProcess(processes, Process.GetCurrentProcess().Id);
            var processManager = LockingProcessManagerFactory.Create();
            processManager.Register(null, processesWithoutSelf);
            return processManager;
        }

        protected override Version? GetComponentVersion(IComponent component)
        {
            try
            {
                return UpdaterUtilities.GetAssemblyFileVersion(component.GetFilePath());
            }
            catch
            {
                return null;
            }
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

        private static IEnumerable<ILockingProcessInfo> WithoutProcess(IEnumerable<ILockingProcessInfo> processes, int processId)
        {
            return processes.Where(x => !x.Id.Equals(processId));
        }

        private static IEnumerable<LauncherUpdaterItem> GetUpdaterItems(IEnumerable<IComponent> components)
        {
            if (components is null)
                throw new ArgumentNullException(nameof(components));
            foreach (var component in components)
            {
                var item = new LauncherUpdaterItem();
                switch (component.RequiredAction)
                {
                    case ComponentAction.Keep:
                        continue;
                    case ComponentAction.Delete:
                        item.File = component.GetFilePath();
                        item.Destination = null;
                        break;
                    case ComponentAction.Update:
                        ComponentDownloadPathStorage.Instance.TryGetValue(component, out var file);
                        item.File = file;
                        item.Destination = component.GetFilePath();
                        break;
                }

                BackupManager.Instance.TryGetValue(component, out var backup);
                item.Backup = backup;
                yield return item;
            }
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}