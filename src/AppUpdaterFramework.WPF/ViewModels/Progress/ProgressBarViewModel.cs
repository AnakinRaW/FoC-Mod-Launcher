using System;
using System.Windows;
using AnakinRaW.AppUpdaterFramework.Updater;
using AnakinRaW.AppUpdaterFramework.Updater.Progress;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnakinRaW.AppUpdaterFramework.ViewModels.Progress;

public abstract partial class ProgressBarViewModel : ViewModelBase, IProgressBarViewModel
{
    [ObservableProperty]
    private ProgressEventArgs? _progressInformation;

    [NotifyChangedIsLinkedToProperty(nameof(ProgressInformation))]
    public abstract string? LeftHeaderText { get; }

    [NotifyChangedIsLinkedToProperty(nameof(ProgressInformation))]
    public abstract string? RightHeaderText { get; }

    [NotifyChangedIsLinkedToProperty(nameof(ProgressInformation))]
    public abstract string? FooterText { get; }

    [NotifyChangedIsLinkedToProperty(nameof(ProgressInformation))]
    public double ProgressValue => ProgressInformation is not null ? Math.Floor(ProgressInformation.Progress * 100.0) : 0.0;

    protected ProgressBarViewModel(IUpdateSession updateSession, string progressEventName, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        WeakEventManager<IUpdateSession, ProgressEventArgs?>.AddHandler(updateSession, progressEventName, OnProgress);
    }

    private void OnProgress(object sender, ProgressEventArgs? e)
    {
        ProgressInformation = e;
    }
}