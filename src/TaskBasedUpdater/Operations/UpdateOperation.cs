using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.Download;
using TaskBasedUpdater.Elevation;
using TaskBasedUpdater.Restart;
using TaskBasedUpdater.TaskRunner;
using TaskBasedUpdater.Tasks;

namespace TaskBasedUpdater.Operations
{
    internal class UpdateOperation : IUpdaterOperation
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly HashSet<IComponent> _allComponents;
        private bool _scheduled;
        private bool? _planSuccessful;
        private readonly List<UpdaterTask> _componentsToInstall = new List<UpdaterTask>();
        private readonly List<UpdaterTask> _componentsToRemove = new List<UpdaterTask>();
        private readonly List<ComponentDownloadTask> _componentsToDownload = new List<ComponentDownloadTask>();
        private readonly ICollection<ElevationRequestData> _elevationRequests = new HashSet<ElevationRequestData>();

        private IEnumerable<IUpdaterTask> _installsOrUninstalls;

        private TaskRunner.TaskRunner _installs;
        private AsyncTaskRunner _downloads;
        private IDownloadManager _downloadManager;
        private AcquireMutexTask? _installMutexTask;

        private CancellationTokenSource? _linkedCancellationTokenSource;
        private Elevator _elevator;
        

        internal bool IsCancelled { get; private set; }

        internal bool RequiredProcessElevation { get; private set; }

        private static int ParallelDownload => 2;

        public UpdateOperation(IEnumerable<IComponent> dependencies)
        {
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

            var downloadLookup = new Dictionary<IComponent, ComponentDownloadTask>();
            foreach (var dependency in _allComponents)
            {
                var packageActivities = PlanInstallable(dependency, downloadLookup);
                if (dependency.RequiredAction == ComponentAction.Delete && packageActivities.Install != null)
                    _componentsToRemove.Add(packageActivities.Install);
            }

            _componentsToRemove.Reverse();
            _installsOrUninstalls = _componentsToRemove.Concat(_componentsToInstall);

            foreach (var installsOrUninstall in _installsOrUninstalls)
            {
                if (installsOrUninstall is ComponentInstallTask install && downloadLookup.ContainsKey(install.Component) && 
                    (install.Action != ComponentAction.Delete|| install.Component.RequiredAction != ComponentAction.Keep))
                    _componentsToDownload.Add(downloadLookup[install.Component]);
            }

            _planSuccessful = true;
            return _planSuccessful.Value;
        }

        public void Run(CancellationToken token = default)
        {
            Schedule();
            var installsOrUninstalls = _installsOrUninstalls?.OfType<ComponentInstallTask>() ?? Enumerable.Empty<ComponentInstallTask>();

            using var mutex = UpdaterUtilities.CheckAndSetGlobalMutex();
            try
            {
                try
                {
                    _linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                    _downloads.Run(_linkedCancellationTokenSource.Token);
                    _installs.Run(_linkedCancellationTokenSource.Token);

                    try
                    {
                        _downloads.Wait();
                    }
                    catch (Exception)
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

                if (RequiredProcessElevation)
                    throw new ElevationRequireException(_elevationRequests);

                if (IsCancelled)
                    throw new OperationCanceledException(token);
                token.ThrowIfCancellationRequested();
                
                var failedDownloads = _componentsToDownload.Where(p =>
                    p.Error != null && !p.Error.IsExceptionType<OperationCanceledException>());

                var failedInstalls = installsOrUninstalls
                    .Where(installTask => !installTask.Result.IsSuccess()).ToList();

                if (failedDownloads.Any() || failedInstalls.Any())
                    throw new ComponentFailedException(
                        "Update failed because one or more downloads or installs had an error.");

                var requiresRestart = LockedFilesWatcher.Instance.LockedFiles.Any();
                if (requiresRestart)
                    Logger.Info("The operation finished. A restart is pedning.");
            }
            finally
            {
                mutex.ReleaseMutex();
                _installMutexTask?.Dispose();
            }
        }

        private void OnElevationRequested(object sender, ElevationRequestData e)
        {
            Logger.Info($"Elevation requested: {e.Exception.Message}");
            RequiredProcessElevation = true;
            _elevationRequests.Add(e);
            if (UpdateConfiguration.Instance.RequiredElevationCancelsUpdate)
                _linkedCancellationTokenSource?.Cancel();
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
            _scheduled = true;
        }

        private void Initialize()
        {
            if (_downloadManager == null)
                _downloadManager = DownloadManager.Instance;

            if (_elevator == null)
            {
                _elevator = Elevator.Instance;
                _elevator.ElevationRequested += OnElevationRequested;
            }
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
        }

        private void CleanEvents()
        {
            if (_downloads != null)
                _downloads.Error -= OnError;
            if (_installs != null)
                _installs.Error -= OnError;
            _elevator.ElevationRequested -= OnElevationRequested;
        }
        
        private void QueueInitialActivities()
        {
            //_installs.Queue(new WaitTask(_downloads)); // Waits until all downloads are finished
            _installMutexTask = new AcquireMutexTask();
            _installs.Queue(_installMutexTask);
        }

        private PackageActivities PlanInstallable(IComponent component, Dictionary<IComponent, ComponentDownloadTask> downloadLookup)
        {
            PackageActivities packageActivities = null;
            if (component != null)
            {
                var isPresent = component.CurrentState == CurrentState.Installed;

                if (component.RequiredAction == ComponentAction.Update || component.RequiredAction == ComponentAction.Keep)
                {
                    // TODO: Debug this and check if everything is correct!!!!
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

        private PackageActivities CreateDownloadInstallActivities(IComponent component, ComponentAction action, bool isPresent)
        {
            ComponentDownloadTask downloadComponent;
            ComponentInstallTask install;

            if (DownloadRequired(action, component))
            {
                downloadComponent = new ComponentDownloadTask(component);
                downloadComponent.Canceled += (_, __) => _linkedCancellationTokenSource?.Cancel();
                install = new ComponentInstallTask(component, action, downloadComponent, isPresent);
            }
            else
            {
                downloadComponent = null;
                install = new ComponentInstallTask(component, action, isPresent);
            }
            
            return new PackageActivities
            {
                Download = downloadComponent,
                Install = install
            };
        }

        private bool DownloadRequired(ComponentAction action, IComponent component)
        {
            if (action != ComponentAction.Update)
                return false;

            if (component.CurrentState == CurrentState.Downloaded && ComponentDownloadPathStorage.Instance.TryGetValue(component, out _))
                return false;


            return true;
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


                if (e.Task is ComponentInstallTask installTask)
                {
                    if (installTask.Result.IsFailure())
                    {
                        // TODO
                    }
                    else
                    {
                        // TODO
                    }
                }
                else if (e.Task is ComponentDownloadTask downloadTask)
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
            internal ComponentDownloadTask Download { get; set; }

            internal ComponentInstallTask Install { get; set; }
        }
    }
}