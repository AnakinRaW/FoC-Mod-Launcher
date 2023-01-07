using System.Collections.ObjectModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.ProductMetadata;

namespace FocLauncher.Update.ViewModels;

public interface IUpdateWindowViewModel : IModalWindowViewModel, ILoadingViewModel
{
    IInstalledProductViewModel InstalledProductViewModel { get; set; }

    ObservableCollection<ProductBranch> Branches { get; }

    ProductBranch CurrentBranch { get; set; }
}