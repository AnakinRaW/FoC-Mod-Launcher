using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.Update.ViewModels;

public interface IInstalledProductViewModel : IViewModel
{
    string DisplayName { get; }

    ImageKey Icon { get; }
}