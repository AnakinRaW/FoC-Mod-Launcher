using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using FocLauncherHost.Updater.Component;
using FocLauncherHost.Updater.FileSystem;
using FocLauncherHost.Updater.Restart;
using NLog;

namespace FocLauncherHost.Updater
{
    internal class FileInstaller
    {
        private static FileInstaller _fileInstaller;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private List<string> _lockedFiles;
        private LockedFileLogger _lockedFileLogger;
        private LockedFilesWatcher _lockedFilesWatcher;

        protected CancellationToken Token { get; private set; }

        protected internal IEnumerable<string> LockedFiles => _lockedFiles;

        protected IComponent CurrentComponent { get; private set; }


        public static FileInstaller Instance => _fileInstaller ??= new FileInstaller();


        private FileInstaller()
        {
            _lockedFilesWatcher = LockedFilesWatcher.Instance;
            _lockedFiles = new List<string>();
        }

        public InstallResult Remove(IComponent component, CancellationToken token, bool isPresent = false)
        {
            Token = token;
            var location = component.Destination;
            return PlanAndApplyExecuteAction(location, component, isPresent);
        }

        protected InstallResult UninstallCore(string installDir, IComponent component)
        {
            return DoAction(() => UninstallCoreInternal(installDir, component));
        }

        internal InstallResult UninstallCoreInternal(string installDir, IComponent component)
        {
            if (!FileSystemExtensions.ContainsPath(installDir, component.Destination))
            {
                Logger.Warn("Different paths for component and method input");
                return InstallResult.Failure;
            }

            var restartPending = false;
            var file = component.GetFilePath();
            var deleteResult = DeleteFile(file, out var restartRequired);
            restartPending |= restartRequired;
            if (!deleteResult && !restartRequired)
                return InstallResult.Failure;

            if (restartPending)
                return InstallResult.SuccessRestartRequired;

            component.CurrentState = CurrentState.Removed;
            return InstallResult.Success;
        }

        protected bool DeleteFile(string file, out bool restartRequired)
        {
            if (!File.Exists(file))
            {
                Logger.Trace($"'{file}' file is already deleted.");
                restartRequired = false;
                return true;
            }

            var deleteSuccess = FileSystemExtensions.DeleteFileWithRetry(file, out restartRequired, true, 2, 500,
                (ex, attempt) =>
                {
                    Logger?.Trace(
                        $"Error occurred while deleting file '{(object) file}'. Error details: {(object) ex.Message}. Retrying after {(object) 0.5f} seconds...");
                    return true;
                });
            if (deleteSuccess)
                Logger.Info($"{file} file deleted.");
            else
            {
                _lockedFiles.Add(file);
                if (restartRequired)
                {
                    Logger.Info($"{file} file is scheduled for deletion after restarting.");
                    if (!_lockedFilesWatcher.LockedFiles.Contains(file))
                        _lockedFilesWatcher.LockedFiles.Add(file);
                }
                else
                    Logger?.Warn($"File '{file}' could not be deleted nor could it be scheduled for deletion until after the reboot.");
            }

            return deleteSuccess;
        }

        protected void PrintReturnCode(InstallResult result, IComponent package, ComponentAction executeAction)
        {
            if (result.IsFailure())
            {
                var installResultDetails = GetInstallResultDetails(result, package, executeAction.ToString().ToLowerInvariant());
                LogFailure(package, executeAction, installResultDetails);
            }
            else
            {
                var text = "Result: " + result;
                Logger.Trace("Package executed successfully. {0}", text);
            }
        }

        protected string GetInstallResultDetails(InstallResult installResult, IComponent component, string action)
        {
            var stringBuilder = new StringBuilder();
            if (stringBuilder.Length > 0) stringBuilder.Append(", ");
            stringBuilder.Append("Result: ");
            stringBuilder.Append(installResult);
            if (component != null)
            {
                var failureSignature = component.GetFailureSignature(action, installResult.ToString());
                stringBuilder.Append(", Signature: ");
                stringBuilder.Append(failureSignature);
            }
            return stringBuilder.ToString();
        }

        private InstallResult UninstallHelper(InstallData uninstallData)
        {
            InstallResult result;
            var component = uninstallData.Component;

            try
            {
                if (component == null && uninstallData.InstallDir == null)
                    result = InstallResult.Failure;
                else
                    result = UninstallCore(uninstallData.InstallDir, component);
            }
            catch (OperationCanceledException)
            {
                Logger.Info("User canceled during component uninstall.");
                return InstallResult.Cancel;
            }
            catch (Exception e)
            {
                LogFailure(component, ComponentAction.Delete, e.ToString());
                return InstallResult.FailureException;
            }
            PrintReturnCode(result, component, ComponentAction.Delete);
            return result;
        }

        private InstallResult DoAction(Func<InstallResult> action)
        {
            Reset();
            var installResult = action();
            try
            {
                LogLockingProcesses();
            }
            catch (Exception ex)
            {
                Logger?.Warn($"Failed to log locking processes: {ex.Message}");
            }
            return installResult;
        }

        private InstallResult PlanAndApplyExecuteAction(string location, IComponent component, bool isPresent)
        {
            var requestedAction = component.RequiredAction;
            var state = component.CurrentState;

            Func<InstallData, InstallResult> action = null;
            switch (requestedAction)
            {
                case ComponentAction.Update:
                    // TODO
                    //action = InstallHelper;
                    break;
                case ComponentAction.Delete:
                    if (isPresent)
                        action = UninstallHelper;
                    break;
            }

            if (action == null)
                return InstallResult.Success;
            try
            {
                CurrentComponent = component;
                return action(new InstallData(component, location));
            }
            finally
            {
                CurrentComponent = null;
            }
        }

        private void LogLockingProcesses()
        {
            if (_lockedFiles.Count <= 0)
                return;
            if (_lockedFileLogger == null) 
                _lockedFileLogger = LockedFileLogger.Instance;
            _lockedFileLogger.Log(_lockedFiles);
        }

        private void Reset()
        {
            this._lockedFiles.Clear();
        }

        private static void LogFailure(IComponent package, ComponentAction executeAction, string details)
        {
            Logger.Error(package != null
                ? $"Package '{package.Name}' failed to {executeAction.ToString().ToLowerInvariant()}. {details}"
                : $"Failed to {executeAction.ToString().ToLowerInvariant()}. {details}");
        }


        private class InstallData
        {
            internal string InstallDir { get; set; }

            //internal string LocalPath { get; set; }

            internal IComponent Component { get; set; }

            internal InstallData(IComponent component, string installDir)
            {
                InstallDir = installDir;
                //LocalPath = localPath;
                Component = component;
            }
        }
    }
}