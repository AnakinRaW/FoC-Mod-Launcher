using System.Collections.ObjectModel;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.CommonUtilities.Wpf.Controls;

namespace FocLauncher.Update.ViewModels;

public interface IUpdateWindowViewModel : IModalWindowViewModel, IViewModel
{
    IInstalledProductViewModel InstalledProductViewModel { get; set; }

    IUpdateInfoBarViewModel InfoBarViewModel { get; }

    ObservableCollection<ProductBranch> Branches { get; }

    ProductBranch CurrentBranch { get; set; }

    bool CanSwitchBranches { get; }
}