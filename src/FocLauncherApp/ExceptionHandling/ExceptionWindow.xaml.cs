using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using FocLauncherApp.Properties;
using Microsoft.Win32;

namespace FocLauncherApp.ExceptionHandling
{
    public partial class ExceptionWindow : INotifyPropertyChanged
    {
        private readonly Window _hostWindow;
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

        public ExceptionWindow() : this(null)
        {          
        }

        public ExceptionWindow(Exception exception)
        {
            InitializeComponent();
            Exception = exception;
            _hostWindow = new Window
            {
                Title = "FoC Launcher",
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            };
        }

        public void ShowDialog()
        {
            SystemSounds.Exclamation.Play();
            _hostWindow.Content = this;
            _hostWindow.ShowDialog();
        }

        private void OnSaveStackTrace(object sender, RoutedEventArgs e)
        {
            if (Exception?.StackTrace == null)
                return;

            var saveFileDialog = new SaveFileDialog {Title = "Save error log", Filter = "Text file (*.txt)|*.txt"};

            if (saveFileDialog.ShowDialog() != true)
                return;

            var sb = new StringBuilder();
            sb.AppendLine("FoC Launcher error log");
            sb.AppendLine();
            sb.AppendLine(Exception.ToString());

            File.WriteAllText(saveFileDialog.FileName, sb.ToString());
            _hostWindow.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }       
    }
}
