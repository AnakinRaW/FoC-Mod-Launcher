using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace FocLauncherHost.Dialogs
{
    public partial class ExceptionWindow : INotifyPropertyChanged
    {
        private Exception _exception;

        public Exception Exception
        {
            get => _exception;
            set
            {
                if (Equals(value, _exception)) return;
                _exception = value;
                OnPropertyChanged();
            }
        }

        public ExceptionWindow(Exception exception)
        {
            InitializeComponent();
            Exception = exception;
        }

        public override void ShowDialog()
        {
            SystemSounds.Exclamation.Play();
            base.ShowDialog();
        }

        private void OnSaveStackTrace(object sender, RoutedEventArgs e)
        {
            if (Exception?.StackTrace == null)
                return;

            var saveFileDialog = new SaveFileDialog {Title = "Save error log", Filter = "Text file (*.txt)|*.txt"};

            if (saveFileDialog.ShowDialog() != true)
                return;

            var sb = new StringBuilder();
            sb.AppendLine("FoC LauncherInformation error log");
            sb.AppendLine();
            sb.AppendLine(Exception.ToString());

            File.WriteAllText(saveFileDialog.FileName, sb.ToString());
            HostWindow.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }       
    }
}
