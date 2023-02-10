using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Update.ViewModels.ProductStates;
using Validation;

namespace FocLauncher.Update.ViewModels;

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