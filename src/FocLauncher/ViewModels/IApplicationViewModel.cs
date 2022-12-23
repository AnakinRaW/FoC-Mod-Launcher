using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace FocLauncher.ViewModels;

public interface IApplicationViewModel : IMainWindowViewModel, ILauncherViewModel
{
    public ILauncherViewModel CurrentViewModel { get; set; }
}