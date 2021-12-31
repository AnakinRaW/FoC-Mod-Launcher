using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace FocLauncher.ViewModels;

public interface IApplicationViewModel : IMainWindowViewModel, ILauncherViewModel
{
    public ILauncherViewModel CurrentViewModel { get; set; }
}