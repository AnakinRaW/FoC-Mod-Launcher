using AnakinRaW.AppUpdaterFramework.ViewModels.ProductStates;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace AnakinRaW.AppUpdaterFramework.ViewModels;

public interface IProductViewModel : IViewModel
{
    string DisplayName { get; }

    ImageKey Icon { get; }

    ICommandDefinition? Action { get; }

    IProductStateViewModel StateViewModel { get; }
}