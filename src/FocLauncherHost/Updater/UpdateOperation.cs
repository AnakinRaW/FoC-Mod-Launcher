using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using FocLauncherHost.Updater.TaskRunner;
using FocLauncherHost.Updater.Tasks;
using NLog;

namespace FocLauncherHost.Updater
{
    internal class UpdateOperation : IUpdaterOperation
    {
        private readonly IProductInfo _product;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly HashSet<IComponent> _allComponents;
        private bool _scheduled;
        private bool? _planSuccessful;
        private readonly List<UpdaterTask> _componentsToInstall = new List<UpdaterTask>();
        private readonly List<UpdaterTask> _componentsToRemove = new List<UpdaterTask>();
        private readonly List<DependencyDownloadTask> _componentsToDownload = new List<DependencyDownloadTask>();

        private IReadOnlyCollection<DependencyDownloadTask> _componentsToDownloadReadOnly;
        private IReadOnlyCollection<UpdaterTask> _componentsToInstallReadOnly;
        private IReadOnlyCollection<UpdaterTask> _componentsToRemoveReadOnly;

        private IEnumerable<IUpdaterTask> _installsOrUninstalls;

        private TaskRunner.TaskRunner _installs;
        private TaskRunner.TaskRunner _cancelRestore;
        private AsyncTaskRunner _downloads;
        private DownloadManager _downloadManager;
        private AcquireMutexTask _installMutexTask;

        private CancellationTokenSource _linkedCancellationTokenSource;


        public IReadOnlyCollection<DependencyDownloadTask> DependenciesToDownload =>
            _componentsToDownloadReadOnly ??= new ReadOnlyCollection<DependencyDownloadTask>(_componentsToDownload);

        public IReadOnlyCollection<UpdaterTask> DependenciesToInstall => _componentsToInstallReadOnly ??=
            new ReadOnlyCollection<UpdaterTask>(_componentsToInstall);

        public IReadOnlyCollection<UpdaterTask> DependenciesToRemove => _componentsToRemoveReadOnly ??=
            new ReadOnlyCollection<UpdaterTask>(_componentsToRemove);

        public long DownloadSize { get; }

        internal bool IsCancelled { get; private set; }

        private int ParallelDownload => 2;

        public UpdateOperation(IProductInfo product, IEnumerable<IComponent> dependencies)
        {
            _product = product;
            _allComponents = new HashSet<IComponent>(dependencies, ComponentIdentityComparer.Default);
        }

        public bool Plan()
        {
            Initialize();
            if (_allComponents.Count == 0)
            {
                var operationException = new InvalidOperationException("No packages were found to install/uninstall.");
                Logger.Error(operationException, operationException.Message);
                _planSuccessful = false;
                return _planSuccessful.Value;
            }

            var downloadLookup = new Dictionary<IComponent, DependencyDownloadTask>();
            foreach (var dependency in _allComponents)
            {
                var packageActivities = PlanInstallable(dependency, downloadLookup);
                ScheduleReplaceActivities(dependency, downloadLookup);
                if (dependency.RequiredAction == ComponentAction.Delete && packageActivities?.Install != null)
                    _componentsToRemove.Add(packageActivities.Install);
            }

            _componentsToRemove.Reverse();
            _installsOrUninstalls = _componentsToRemove.Concat(_componentsToInstall);

            foreach (var installsOrUninstall in _installsOrUninstalls)
            {
                if (installsOrUninstall is DependencyInstallTask install && downloadLookup.ContainsKey(install.Component) && 
                    (install.InstallAction != InstallAction.Remove|| install.Component.RequiredAction != ComponentAction.Keep))
                    _componentsToDownload.Add(downloadLookup[install.Component]);
            }

            _planSuccessful = true;
            return _planSuccessful.Value;
        }

        public void Run(CancellationToken token = default)
        {
            Schedule();
            var installsOrUninstalls = _installsOrUninstalls?.OfType<DependencyInstallTask>() ?? Enumerable.Empty<DependencyInstallTask>();
            
            using var mutex = UpdaterUtilities.CheckAndSetGlobalMutex();
            try
            {
                try
                {
                    _linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                    _downloads.Run(_linkedCancellationTokenSource.Token);
#if DEBUG
                    _downloads.Wait();
#endif
                    _installs.Run(_linkedCancellationTokenSource.Token);

                    try
                    {
                        _downloads.Wait();
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    if (_linkedCancellationTokenSource != null)
                    {
                        _linkedCancellationTokenSource.Dispose();
                        _linkedCancellationTokenSource = null;
                    }
                    Logger.Trace("Completed update operation");
                }
                
                if (IsCancelled)
                    throw new OperationCanceledException(token);
                token.ThrowIfCancellationRequested();

                var failedDownloads = _componentsToDownload.Where(p => p.Error != null && !p.Error.IsExceptionType<OperationCanceledException>());

                var failedInstalls = installsOrUninstalls
                    .Where(installTask => installTask.Result.IsFailure()).ToList();


                // TODO: Check for any restart requests
                var requiresRestart = false;

                // TODO: Do something if restart is requested

                if (!requiresRestart)
                {
                    if (failedDownloads.Any() || failedInstalls.Any())
                        throw new InvalidOperationException("Update Failed");
                }
                // TODO: Somehwere here but after checking for restart should be a cleanup of backus....
                // If we get here everything should be ready to remove backups

            }
            catch (OperationCanceledException ex)
            {
                Logger.Trace("Running cancellation cleanup activities");
                _cancelRestore.Run(new CancellationToken());
                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
                _installMutexTask?.Dispose();
            }
        }

        internal void Schedule()
        {
            if (_scheduled)
                return;
            Initialize();
            if (!Plan())
                return;

            _componentsToDownload.ForEach(download => _downloads.Queue(download));

            QueueInitialActivities();
            foreach (var installsOrUninstall in _installsOrUninstalls)
                _installs.Queue(installsOrUninstall);

            // TODO _installs might be wrong for our case. A _cleanup might be better
            //QueueCompletionActivities(_cleanup);
            QueueCancelCleanupActivities(_cancelRestore);

            _scheduled = true;
        }

        private void Initialize()
        {
            if (_downloadManager == null)
                _downloadManager = DownloadManager.Instance;

            if (_downloads == null)
            {
                var workers = ParallelDownload;
                Logger?.Trace($"Concurrent downloads: {workers}");
                _downloads = new AsyncTaskRunner(workers);
                _downloads.Error += OnError;
            }
            if (_installs == null)
            {
                _installs = new TaskRunner.TaskRunner();
                _installs.Error += OnError;
            }
            if (_cancelRestore == null)
            {
                _cancelRestore = new TaskRunner.TaskRunner();
                _cancelRestore.Error += OnError;
            }
            // TODO: Add _cleanup
            //if (_cleanup == null)
            //{
            //    _cleanup = new UpdateTaskRunner();
            //    _cleanup.Error += OnError;
            //}
        }
        
        private void QueueInitialActivities()
        {
            //_installs.Queue(new WaitTask(_downloads)); // Waits until all downloads are finished
            _installMutexTask = new AcquireMutexTask();
            _installs.Queue(_installMutexTask);
        }

        private void QueueCompletionActivities(TaskRunner.TaskRunner installs)
        {
            // TODO: Probably remove backup files?!
        }

        private void QueueCancelCleanupActivities(TaskRunner.TaskRunner taskRunner)
        {
            var instanceOnCancel = new UpdaterOnCancelTask();
            taskRunner.Queue(instanceOnCancel);
        }


        private PackageActivities PlanInstallable(IComponent component, Dictionary<IComponent, DependencyDownloadTask> downloadLookup)
        {
            PackageActivities packageActivities = null;
            if (component != null)
            {
                var isPresent = component.CurrentState == CurrentState.Installed;

                if (component.RequiredAction == ComponentAction.Update ||
                    component.RequiredAction == ComponentAction.Keep)
                {
                    // TODO: Debug this and check if everything is correct!!!!
                    isPresent = false;

                    packageActivities = CreateDownloadInstallActivities(component, component.RequiredAction, isPresent);
                    if (packageActivities.Install != null)
                        _componentsToInstall.Add(packageActivities.Install);
                    if (packageActivities.Download != null)
                        downloadLookup[component] = packageActivities.Download;
                }

                if (component.RequiredAction == ComponentAction.Delete)
                {
                    packageActivities = CreateDownloadInstallActivities(component, component.RequiredAction, isPresent);
                    if (packageActivities.Download != null)
                        downloadLookup[component] = packageActivities.Download;
                }

            }

            return packageActivities;
        }

        private void ScheduleReplaceActivities(IComponent component, Dictionary<IComponent, DependencyDownloadTask> downloadLookup)
        {
            var isPresent = component.CurrentState == CurrentState.Installed;
             if (!isPresent || component.RequiredAction != ComponentAction.Keep)
                return;

            var packageActivities = CreateDownloadInstallActivities(component, ComponentAction.Delete, true);
            if (packageActivities.Download != null)
                downloadLookup[component] = packageActivities.Download;
            if (packageActivities.Install != null)
                _componentsToRemove.Add(packageActivities.Install);
        }

        private PackageActivities CreateDownloadInstallActivities(IComponent component, ComponentAction action, bool isPresent)
        {
            DependencyDownloadTask downloadPackage;
            DependencyInstallTask install;

            if (action == ComponentAction.Update)
            {
                downloadPackage = new DependencyDownloadTask();
                install = new DependencyInstallTask(component);
            }
            else
            {
                downloadPackage = null;
                install = new DependencyInstallTask(component);
            }
            
            return new PackageActivities
            {
                Download = downloadPackage,
                Install = install
            };
        }

        private void OnError(object sender, TaskEventArgs e)
        {
            IsCancelled |= e.Cancel;
            if (e.Cancel)
                _linkedCancellationTokenSource?.Cancel();
            try
            {
                if (e.Cancel || e.Task.Component == null)
                    return;

                if (e.Task is DependencyInstallTask installTask)
                {
                    // TODO
                }
                else if (e.Task is DependencyDownloadTask downloadTask)
                {
                    // TODO
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Skipping as an error occurred");
            }
        }

        private class PackageActivities
        {
            internal DependencyDownloadTask Download { get; set; }

            internal DependencyInstallTask Install { get; set; }
        }
    }
}