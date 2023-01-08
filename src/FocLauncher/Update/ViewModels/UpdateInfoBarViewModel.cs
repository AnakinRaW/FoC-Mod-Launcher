using System;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace FocLauncher.Update.ViewModels;

public class UpdateInfoBarViewModel : ViewModelBase, IUpdateInfoBarViewModel
{
    public UpdateStatus Status { get; set; }

    [NotifyChangedIsLinkedToProperty(nameof(Status))]
    public string Text { get; }

    [NotifyChangedIsLinkedToProperty(nameof(Status))]
    public bool IsCheckingForUpdates => Status == UpdateStatus.CheckingForUpdate;

    public UpdateInfoBarViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}