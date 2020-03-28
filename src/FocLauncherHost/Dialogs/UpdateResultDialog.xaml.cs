using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FocLauncherHost.Dialogs
{
    public partial class UpdateResultDialog : INotifyPropertyChanged
    {
        private string _message;
        private string _title;

        public string Title
        {
            get => _title;
            set
            {
                if (value.Equals(_title))
                    return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                if(value.Equals(_message))
                    return;
                _message = value;
                OnPropertyChanged();
            }
        }

        public UpdateResultDialog(string title, string message)
        {
            InitializeComponent();
            Title = title;
            Message = message;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
