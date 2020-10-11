using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FocLauncherHost.Dialogs
{
    public partial class RestoreDialog : INotifyPropertyChanged
    {
        private const string RequiresRestoreMessage =
            "The update of the launcher failed so hard that it needs to be set to an initial state. Press 'Restore now' to fix the problem.";

        private const string WasRestoredMessage =
            "The update of the launcher failed so hard that it had to reset itself.";

        private string _message = string.Empty;
        private bool _requiresRestore;

        public bool RestoreNow { get; private set; }

        public bool RequiresRestore
        {
            get => _requiresRestore;
            private set
            {
                if (value == _requiresRestore)
                    return;
                _requiresRestore = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                if (value.Equals(_message))
                    return;
                _message = value;
                OnPropertyChanged();
            }
        }

        public RestoreDialog(bool requiresRestore)
        {
            InitializeComponent();
            Message = requiresRestore ? RequiresRestoreMessage : WasRestoredMessage;
            RequiresRestore = requiresRestore;
        }

        private void OnRestore(object sender, RoutedEventArgs e)
        {
            RestoreNow = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
