using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Application;

namespace FocLauncher.ViewModels;

public interface IApplicationViewModel : IMainWindowViewModel, ILauncherViewModel
{
    public ILauncherViewModel CurrentViewModel { get; set; }
}