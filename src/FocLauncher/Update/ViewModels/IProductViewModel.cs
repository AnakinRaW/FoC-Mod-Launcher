using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Update.ViewModels.ProductStates;

namespace FocLauncher.Update.ViewModels;

public interface IProductViewModel : IViewModel
{
    string DisplayName { get; }

    ImageKey Icon { get; }

    ICommandDefinition? Action { get; }

    IProductStateViewModel StateViewModel { get; }
}