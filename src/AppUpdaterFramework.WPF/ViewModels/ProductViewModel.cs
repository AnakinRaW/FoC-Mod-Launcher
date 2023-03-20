using System;
using AnakinRaW.AppUpdaterFramework.ViewModels.ProductStates;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.ViewModels;

public class ProductViewModel : ViewModelBase, IProductViewModel
{
    public string DisplayName { get; }

    public ImageKey Icon { get; }

    public IProductStateViewModel StateViewModel { get; }

    public ICommandDefinition? Action { get; }

    public ProductViewModel(string displayName, ImageKey icon, IProductStateViewModel stateViewModel, ICommandDefinition? action, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Requires.NotNull(stateViewModel, nameof(stateViewModel));
        StateViewModel = stateViewModel;
        Action = action;
        DisplayName = displayName;
        Icon = icon;
    }
}