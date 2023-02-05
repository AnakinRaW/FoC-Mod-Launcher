using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.Update.ViewModels;

public interface IInstalledProductViewModel : IViewModel
{
    string DisplayName { get; }

    ImageKey Icon { get; }

    ICommandDefinition? Action { get; }

    IInstalledProductStateViewModel StateViewModel { get; }
}