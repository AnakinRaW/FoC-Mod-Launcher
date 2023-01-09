using System.Collections.ObjectModel;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.CommonUtilities.Wpf.Controls;
using AnakinRaW.ProductMetadata;

namespace FocLauncher.Update.ViewModels;

public interface IUpdateWindowViewModel : IModalWindowViewModel, IViewModel
{
    IInstalledProductViewModel InstalledProductViewModel { get; set; }

    IUpdateInfoBarViewModel InfoBarViewModel { get; }

    ObservableCollection<ProductBranch> Branches { get; }

    ProductBranch CurrentBranch { get; set; }

    bool IsLoadingBranches { get; set; }
}