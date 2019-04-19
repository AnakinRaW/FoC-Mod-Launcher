using System.ComponentModel;
using System.Runtime.CompilerServices;
using FocLauncher.Annotations;

namespace FocLauncher
{
    public class LauncherDataModel : IDataModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}