using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FocLauncher;
using FocLauncherHost.Properties;
using FocLauncherHost.Update.UpdateCatalog;
using FocLauncherHost.Updater;
using FocLauncherHost.Updater.Component;
using FocLauncherHost.Updater.Restart;
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

        private static void SetUpdateConfiguration()
        {
            UpdateConfiguration.Instance.BackupPolicy = BackupPolicy.Required;
            UpdateConfiguration.Instance.DownloadRetryCount = 1;
            UpdateConfiguration.Instance.BackupPath = LauncherConstants.ApplicationBasePath;
            UpdateConfiguration.Instance.DownloadRetryDelay = 500;
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

        protected override async Task HandleRestartRequestAsync(ICollection<IComponent> pendingComponents, ILockingProcessManager lockingProcessManager,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!pendingComponents.Any())
                return;

            Logger.Trace("Hanlde restart request due to locked files");

            var processes = lockingProcessManager.GetProcesses().ToList();
            if (processes.Any(x => x.ApplicationType == ApplicationType.Critical))
                throw new RestartDeniedOrFailedException("Files are locked by a system process that cannot be terminated. Please restart the system");

            var isSelfLocking = ProcessesContainsLauncher(processes);
            var restartRequestResult = LauncherRestartManager.ShowProcessKillDialog(lockingProcessManager, token);

            Logger.Trace($"Kill locking processes: {restartRequestResult}, Launcher needs restart: {isSelfLocking}");

            if (!restartRequestResult)
                throw new RestartDeniedOrFailedException("Update aborted because locked files have not been released.");

            if (!isSelfLocking)
            {
                lockingProcessManager.Shutdown();
                LockedFilesWatcher.Instance.LockedFiles.Clear();
                await UpdateAsync(pendingComponents, token);
                if (LockedFilesWatcher.Instance.LockedFiles.Any())
                    throw new RestartDeniedOrFailedException(
                        "Update failed because there are still locked files which have not been released.");
                return;
            }
            // TODO: Using the process manager might result in that this process is also shutted down. Test the behaviour!
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

        private static bool ProcessesContainsLauncher(IEnumerable<ILockingProcessInfo> processes)
        {
            var currentProcess = Process.GetCurrentProcess();
            return processes.Any(x => x.Id.Equals(currentProcess.Id));
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