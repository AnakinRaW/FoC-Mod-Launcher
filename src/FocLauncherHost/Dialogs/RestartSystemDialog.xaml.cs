using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FocLauncherHost.Dialogs
{
    public partial class RestartSystemDialog : INotifyPropertyChanged
    {
        private string _exceptionMessage;

        public string ExceptionMessage
        {
            get => _exceptionMessage;
            set
            {
                if (value.Equals(_exceptionMessage))
                    return;
                _exceptionMessage = value;
                OnPropertyChanged();
            }
        }

        public RestartSystemDialog(string message)
        {
            InitializeComponent();
            ExceptionMessage = message;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
