using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using Validation;

namespace FocLauncher.Update.ViewModels;

public class InstalledProductViewModel : ViewModelBase, IInstalledProductViewModel
{
    public string DisplayName { get; }

    public ImageKey Icon { get; }

    public IInstalledProductStateViewModel StateViewModel { get; }

    public ICommandDefinition? Action { get; }

    public InstalledProductViewModel(string displayName, ImageKey icon, IInstalledProductStateViewModel stateViewModel, ICommandDefinition? action, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Requires.NotNull(stateViewModel, nameof(stateViewModel));
        StateViewModel = stateViewModel;
        Action = action;
        DisplayName = displayName;
        Icon = icon;
    }
}