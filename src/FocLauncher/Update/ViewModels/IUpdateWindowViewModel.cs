using System.Collections.ObjectModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.ProductMetadata;

namespace FocLauncher.Update.ViewModels;

public interface IUpdateWindowViewModel : IModalWindowViewModel, IViewModel
{
    IInstalledProductViewModel InstalledProductViewModel { get; set; }

    IUpdateInfoBarViewModel InfoBarViewModel { get; }

    ObservableCollection<ProductBranch> Branches { get; }

    ProductBranch CurrentBranch { get; set; }

    bool IsLoadingBranches { get; set; }
}