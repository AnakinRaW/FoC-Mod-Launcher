using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;

namespace FocLauncher.WaitDialog
{
    internal class WaitDialogDataSource : ObservableObject
    {
        private string _caption;
        private string _waitMessage;
        private string _progressMessage;
        private bool _isProgressVisible;
        private bool _showMarqueeProgress;
        private bool _isCancellable;
        private int _currentStep;
        private int _totalSteps;
        private Brush _foregroundColorBrush;
        private Brush _backgroundColorBrush;
        private Brush _borderColorBrush;
        private Brush _captionForegroundColorBrush;
        private Brush _captionBackgroundColorBrush;
        private string _cancelText;

        public Brush ForegroundColorBrush
        {
            get => _foregroundColorBrush;
            set => SetProperty(ref _foregroundColorBrush, value, nameof(ForegroundColorBrush));
        }

        public Brush BackgroundColorBrush
        {
            get => _backgroundColorBrush;
            set => SetProperty(ref _backgroundColorBrush, value, nameof(BackgroundColorBrush));
        }

        public Brush BorderColorBrush
        {
            get => _borderColorBrush;
            set => SetProperty(ref _borderColorBrush, value, nameof(BorderColorBrush));
        }

        public Brush CaptionForegroundColorBrush
        {
            get => _captionForegroundColorBrush;
            set => SetProperty(ref _captionForegroundColorBrush, value, nameof(CaptionForegroundColorBrush));
        }

        public Brush CaptionBackgroundColorBrush
        {
            get => _captionBackgroundColorBrush;
            set => SetProperty(ref _captionBackgroundColorBrush, value, nameof(CaptionBackgroundColorBrush));
        }

        public string Caption
        {
            get => _caption;
            set => SetProperty(ref _caption, value, nameof(Caption));
        }

        public string WaitMessage
        {
            get => _waitMessage;
            set => SetProperty(ref _waitMessage, value, nameof(WaitMessage));
        }

        public string ProgressMessage
        {
            get => _progressMessage;
            set => SetProperty(ref _progressMessage, value, nameof(ProgressMessage));
        }

        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set => SetProperty(ref _isProgressVisible, value, nameof(IsProgressVisible));
        }

        public bool ShowMarqueeProgress
        {
            get => _showMarqueeProgress;
            set => SetProperty(ref _showMarqueeProgress, value, nameof(ShowMarqueeProgress));
        }

        public bool IsCancellable
        {
            get => _isCancellable;
            set => SetProperty(ref _isCancellable, value, nameof(IsCancellable));
        }

        public int CurrentStep
        {
            get => _currentStep;
            set => SetProperty(ref _currentStep, value, nameof(CurrentStep));
        }

        public int TotalSteps
        {
            get => _totalSteps;
            set => SetProperty(ref _totalSteps, value, nameof(TotalSteps));
        }

        public string CancelText
        {
            get => _cancelText;
            set => SetProperty(ref _cancelText, value, nameof(CancelText));
        }
    }
}