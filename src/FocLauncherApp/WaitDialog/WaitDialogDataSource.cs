using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FocLauncherApp.WaitDialog
{
    internal class WaitDialogDataSource : INotifyPropertyChanged
    {
        private string _caption;
        private string _waitMessage;
        private string _progressMessage;
        private bool _isProgressVisible;
        private bool _showMarqueeProgress;
        private bool _isCancellable;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Caption
        {
            get => _caption;
            set
            {
                if (value == _caption) return;
                _caption = value;
                OnPropertyChanged();
            }
        }

        public string WaitMessage
        {
            get => _waitMessage;
            set
            {
                if (value == _waitMessage) return;
                _waitMessage = value;
                OnPropertyChanged();
            }
        }

        public string ProgressMessage
        {
            get => _progressMessage;
            set
            {
                if (value == _progressMessage) return;
                _progressMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set
            {
                if (value == _isProgressVisible) return;
                _isProgressVisible = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsCancellable
        {
            get => _isCancellable;
            set
            {
                if (value == _isCancellable) return;
                _isCancellable = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}