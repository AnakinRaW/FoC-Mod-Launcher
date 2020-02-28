using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using FocLauncherHost.Updater.Restart;

namespace FocLauncherHost.Controls
{
    public partial class UpdateMessageBox : INotifyPropertyChanged
    {
        private string _buttonText;
        public event PropertyChangedEventHandler PropertyChanged;

        public ProcessKillResult Result { get; private set; }

        public string ButtonText
        {
            get => _buttonText;
            set
            {
                if (_buttonText == value)
                    return;
                _buttonText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ILockingProcessInfo> Processes { get; }

        public UpdateMessageBox(IEnumerable<ILockingProcessInfo> processes)
        {
            Processes = new ObservableCollection<ILockingProcessInfo>(processes);
            ButtonText = Processes.Count == 1 ? "End Process" : "End Processes";
            InitializeComponent();
        }

        public override void ShowDialog()
        {
            HostWindow.Topmost = true;
            HostWindow.Owner = Application.Current.MainWindow;
            base.ShowDialog();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnRetry(object sender, RoutedEventArgs e)
        {
            Result = ProcessKillResult.Retry;
            HostWindow.Close();
        }

        private void OnProceed(object sender, RoutedEventArgs e)
        {
            Result = ProcessKillResult.Kill;
            HostWindow.Close();
        }

        public enum ProcessKillResult
        {
            Abort,
            Kill,
            Retry
        }
    }
}
