using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using FocLauncher.Game;

namespace FocLauncherHost
{
    public partial class SplashScreen : INotifyPropertyChanged
    {
        private bool _isProgressVisible;
        private string _progressText;
        private bool _cancelable = true;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _isBeta;

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public bool IsBeta
        {
            get => _isBeta;
            set
            {
                if (value == _isBeta)
                    return;
                _isBeta = value;
                OnPropertyChanged();
            }
        }

        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set
            {
                if (value == _isProgressVisible)
                    return;
                _isProgressVisible = value;
                OnPropertyChanged();
            }
        }

        public string ProgressText
        {
            get => _progressText;
            set
            {
                if (value.Equals(_progressText))
                    return;
                _progressText = value;
                OnPropertyChanged();
            }
        }

        public bool Cancelable
        {
            get => _cancelable;
            set
            {
                if (value == _cancelable)
                    return;
                _cancelable = value;
                OnPropertyChanged();
            }
        }

        public SplashScreen()
        {
            InitializeComponent();
        }

        public Task HideAnimationAsync()
        {
            var storyboard = FindResource("HideAnimation") as Storyboard;
            var tcs = new TaskCompletionSource<bool>();
            if (storyboard == null)
                tcs.SetException(new ArgumentNullException());
            else
            {
                EventHandler onComplete = null;
                onComplete = (s, e) => {
                    storyboard.Completed -= onComplete;
                    tcs.SetResult(true);
                };
                storyboard.Completed += onComplete;
                storyboard.Begin(this);
            }
            return tcs.Task;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            if (_cancelable)
                _cancellationTokenSource.Cancel();
        }
    }
}
