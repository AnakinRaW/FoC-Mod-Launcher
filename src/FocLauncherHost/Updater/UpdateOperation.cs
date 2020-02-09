using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using FocLauncherHost.Updater.MetadataModel;
using FocLauncherHost.Updater.Tasks;
using NLog;

namespace FocLauncherHost.Updater
{
    internal class UpdateOperation : IUpdaterOperation
    {
        private readonly IProductInfo _product;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly HashSet<Dependency> _allDependencies;
        private bool _scheduled;
        private bool? _planSuccessful;
        private readonly List<UpdateTask> _dependenciesToInstall = new List<UpdateTask>();
        private readonly List<UpdateTask> _dependenciesToRemove = new List<UpdateTask>();
        private readonly List<UpdateTask> _dependenciesToDownload = new List<UpdateTask>();

        private IReadOnlyCollection<UpdateTask> _dependenciesToDownloadReadOnly;
        private IReadOnlyCollection<UpdateTask> _dependenciesToInstallReadOnly;
        private IReadOnlyCollection<UpdateTask> _dependenciesToRemoveReadOnly;

        public IReadOnlyCollection<UpdateTask> DependenciesToDownload =>
            _dependenciesToDownloadReadOnly ??= new ReadOnlyCollection<UpdateTask>(_dependenciesToDownload);

        public IReadOnlyCollection<UpdateTask> DependenciesToInstall => _dependenciesToInstallReadOnly ??=
            new ReadOnlyCollection<UpdateTask>(_dependenciesToInstall);

        public IReadOnlyCollection<UpdateTask> DependenciesToRemove => _dependenciesToRemoveReadOnly ??=
            new ReadOnlyCollection<UpdateTask>(_dependenciesToRemove);

        public long DownloadSize { get; }

        public UpdateOperation(IProductInfo product, IEnumerable<Dependency> dependencies)
        {
            _product = product;
            _allDependencies = new HashSet<Dependency>(dependencies);
        }

        public bool Plan()
        {
            if (_allDependencies.Count == 0)
            {
                var operationException = new InvalidOperationException("No packages were found to install/uninstall.");
                Logger.Error(operationException, operationException.Message);
                _planSuccessful = false;
                return _planSuccessful.Value;
            }

            foreach (var task in _allDependencies)
            {
                
            }

            return false;
        }

        public void Run(CancellationToken token = default)
        {
            Schedule();
        }

        internal void Schedule()
        {
            if (_scheduled)
                return;
            if (!Plan())
                return;

            _scheduled = true;
        }

        private void Initialize()
        {
        }
    }
}