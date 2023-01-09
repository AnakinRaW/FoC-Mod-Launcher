using System.ComponentModel;
using System.Threading.Tasks;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

public interface IViewModel : INotifyPropertyChanged
{
    Task InitializeAsync();
}