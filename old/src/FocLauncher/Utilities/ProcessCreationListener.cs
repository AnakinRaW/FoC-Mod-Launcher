using System;
using System.Globalization;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace FocLauncher.Utilities
{
    public class ProcessCreationListener : IDisposable
    {
		public event EventHandler? Started;
        private ManagementEventWatcher _watcher;
        private bool _disposed;
        private readonly object _syncObject = new object();

        private bool _started;

        private string Query { get; }

        private static string BuildQuery(string processName, double pollingTime)
        {
            var pol = pollingTime.ToString("F", CultureInfo.InvariantCulture);
            return "SELECT *" +
                   "  FROM __InstanceCreationEvent " +
                   "WITHIN  " + pol +
                   " WHERE TargetInstance ISA 'Win32_Process' " +
                   "   AND TargetInstance.Name = '" + processName + "'";
        }


        public ProcessCreationListener(string processName, bool startImmediately) : this(processName, 0.25, startImmediately)
        {
        }

        public ProcessCreationListener(string processName, double pollingTimeSeconds, bool startImmediately)
        {
            Query = BuildQuery(processName, pollingTimeSeconds);
            _watcher = new ManagementEventWatcher(@"\\.\root\CIMV2", Query);
            if (startImmediately)
                Start();
        }

        public void Start()
        {
            if (_started)
                return;
            lock (_syncObject)
            {
                _watcher.EventArrived += OnEventArrived;
                _watcher.Start();
            }
            _started = true;
        }

        public void Stop()
        {
            try
            {
                lock (_syncObject)
                {
                    _watcher.EventArrived -= OnEventArrived;
                    _watcher.Stop();
                }
            }
            finally
            {
                _started = false;
            }
        }


		~ProcessCreationListener()
        {
			Dispose(false);
        }

        public static async Task WaitProcessCreatedAsync(string processName, CancellationToken token = default)
        {
            var m = new AsyncManualResetEvent();
            using var listener = new ProcessCreationListener(processName, true);
            listener.Started += delegate { m.Set(); };
            await m.WaitAsync(token);
        }

        public void Dispose()
		{
			Dispose(false);
			GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
			if (_disposed || !disposing)
				return;
            if (_watcher != null)
            {
                Stop();
                _watcher.Dispose();
            }
            _watcher = null;
            _disposed = true;
        }

        private void OnEventArrived(object sender, EventArrivedEventArgs e)
		{
			try 
			{
				var eventName = e.NewEvent.ClassPath.ClassName;

				if (string.Compare(eventName, "__InstanceCreationEvent", StringComparison.Ordinal)==0)
                    Started?.Invoke(this, e);
            }
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}
		
	}
}
