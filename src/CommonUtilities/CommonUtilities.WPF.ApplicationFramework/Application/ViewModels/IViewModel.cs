using System.ComponentModel;
using System.Threading.Tasks;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

public interface IViewModel : INotifyPropertyChanged
{
    Task InitializeAsync();
}