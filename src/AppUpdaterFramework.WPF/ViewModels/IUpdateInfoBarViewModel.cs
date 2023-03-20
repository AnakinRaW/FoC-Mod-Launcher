using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace AnakinRaW.AppUpdaterFramework.ViewModels;

public interface IUpdateInfoBarViewModel : IViewModel, IDisposable
{
    UpdateStatus Status { get; set; }

    string Text { get; }

    bool IsCheckingForUpdates { get; }
}