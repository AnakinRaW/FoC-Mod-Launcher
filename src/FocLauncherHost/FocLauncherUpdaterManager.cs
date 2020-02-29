﻿using System;
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

        protected override async Task<HandleRestartResult> HandleRestartRequestAsync(ICollection<IComponent> pendingComponents, ILockingProcessManager lockingProcessManager,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!pendingComponents.Any())
                return new HandleRestartResult(HandleRestartStatus.NotRequired);

            Logger.Trace("Hanlde restart request due to locked files");

            var processes = lockingProcessManager.GetProcesses().ToList();

            var isSelfLocking = ProcessesContainsLauncher(processes);

            if (!isSelfLocking && processes.Any(x => x.ApplicationType == ApplicationType.Critical))
                return new HandleRestartResult(HandleRestartStatus.Declined, "Files are locked by a system process that cannot be terminated. Please restart the system");

            
            if (!isSelfLocking)
            {
                var restartRequestResult = LauncherRestartManager.ShowProcessKillDialog(lockingProcessManager, token);
                Logger.Trace($"Kill locking processes: {restartRequestResult}, Launcher needs restart: {false}");
                if (!restartRequestResult)
                    return new HandleRestartResult(HandleRestartStatus.Declined, "Update aborted because locked files have not been released.");

                lockingProcessManager.Shutdown();
                LockedFilesWatcher.Instance.LockedFiles.Clear();
                await UpdateAsync(pendingComponents, token);
                return LockedFilesWatcher.Instance.LockedFiles.Any()
                    ? new HandleRestartResult(HandleRestartStatus.Declined,
                        "Update failed because there are still locked files which have not been released.")
                    : new HandleRestartResult(HandleRestartStatus.NotRequired);
            }

            var result = LauncherRestartManager.ShowSelfKillDialog(lockingProcessManager, token);
            Logger.Trace($"Kill locking processes: {result}, Launcher needs restart: {true}");
            if (!result)
                return new HandleRestartResult(HandleRestartStatus.Declined,
                    "Update aborted because locked files have not been released.");

            var processesWithoutSelf = WithoutProcess(processes, Process.GetCurrentProcess().Id);
            using var newLockingProcessManager = LockingProcessManager.Create();
            newLockingProcessManager.Register(null, processesWithoutSelf);
            newLockingProcessManager.Shutdown();
            return new HandleRestartResult(HandleRestartStatus.Restart);
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

        private static IEnumerable<ILockingProcessInfo> WithoutProcess(IEnumerable<ILockingProcessInfo> processes, int processId)
        {
            return processes.Where(x => !x.Id.Equals(processId));
        }
    }
}