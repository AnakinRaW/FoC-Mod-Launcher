using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FocLauncherHost.Dialogs
{
    public partial class UpdateSuccessDialog : INotifyPropertyChanged
    {
        public UpdateSuccessDialog()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
