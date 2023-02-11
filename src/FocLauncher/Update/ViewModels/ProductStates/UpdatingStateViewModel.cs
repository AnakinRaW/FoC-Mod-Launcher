using System;
using AnakinRaW.AppUpaterFramework.Updater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ProgressBar;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace FocLauncher.Update.ViewModels.ProductStates;

public class UpdatingStateViewModel : ViewModelBase, IUpdatingStateViewModel
{
    public IProgressBarViewModel DownloadProgressBarViewModel { get; }
    public IProgressBarViewModel UpdateProgressBarViewModel { get; }

    public UpdatingStateViewModel(IServiceProvider serviceProvider, IUpdateSession updateSession) : base(serviceProvider)
    {
    }
}