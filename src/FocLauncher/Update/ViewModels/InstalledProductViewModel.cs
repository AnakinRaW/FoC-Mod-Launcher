using System;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.Update.ViewModels;

public class InstalledProductViewModel : ViewModelBase, IInstalledProductViewModel
{
    public string DisplayName { get; }

    public ImageKey Icon { get; }

    public IProductInformationViewModel InformationViewModel { get; }

    public InstalledProductViewModel(string displayName, ImageKey icon, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        DisplayName = displayName;
        Icon = icon;
    }
}