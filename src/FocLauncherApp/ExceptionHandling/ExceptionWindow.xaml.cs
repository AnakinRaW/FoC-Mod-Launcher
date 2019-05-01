using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using FocLauncherApp.Properties;
using Microsoft.Win32;

namespace FocLauncherApp.ExceptionHandling
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

        public ExceptionWindow() : this(null)
        {          
        }

        public ExceptionWindow(Exception exception)
        {
            InitializeComponent();
            Exception = exception;
        }

        private void OnSaveStackTrace(object sender, RoutedEventArgs e)
        {
            if (Exception?.StackTrace == null)
                return;

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }       
    }
}
