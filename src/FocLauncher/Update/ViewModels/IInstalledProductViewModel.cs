using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.Update.ViewModels;

public interface IInstalledProductViewModel : IViewModel
{
    string DisplayName { get; }

    ImageKey Icon { get; }
}