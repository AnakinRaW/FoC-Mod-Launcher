using System.ComponentModel;
using System.Threading.Tasks;

namespace FocLauncher.ViewModels;
public interface ILauncherViewModel : INotifyPropertyChanged
{ 
    Task InitializeAsync();
}