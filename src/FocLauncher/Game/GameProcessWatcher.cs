using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FocLauncher.Game
{
    public sealed class GameProcessWatcher
    {
        private readonly object _syncObject = new object();

        internal event EventHandler ProcessExited;

        public GameProcessWatcher()
        {
            IsProcessRunning = false;
        }

        public bool IsProcessRunning { get; private set; }


        private Process _process;
        private bool _isProcessRunning;

        internal void SetProcess(Process process)
        {
            if (process == null || process.HasExited)
                throw new ArgumentException("Game process must not be null or already be closed");
            Process = process;
        }

        private Process Process
        {
            set
            {
                lock (_syncObject)
                {
                    if (Equals(value, _process))
                        return;
                    if (_process != null)
                        _process.Exited -= Process_Exited;
                    if (value == null)
                    {
                        _process = null;
                        return;
                    }
                    _process = value;
                    _process.EnableRaisingEvents = true;
                    IsProcessRunning = ProcessHelper.IsProcessWithNameAlive(_process);
                    _process.Exited += Process_Exited;
                }
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            if (sender is Process process)
            {
                process.Exited -= Process_Exited;
                process.Dispose();
            }
            OnProcessExited();
            lock (_syncObject)
            {
                IsProcessRunning = false;
            }
            Process = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnProcessExited()
        {
            ProcessExited?.Invoke(this, EventArgs.Empty);
        }
    }
}
