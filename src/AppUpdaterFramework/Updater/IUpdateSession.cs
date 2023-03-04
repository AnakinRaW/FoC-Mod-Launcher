using System;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Updater.Progress;

namespace AnakinRaW.AppUpdaterFramework.Updater;

public interface IUpdateSession
{
    event EventHandler<ProgressEventArgs?> DownloadProgress;

    event EventHandler<ProgressEventArgs?> InstallProgress;

    IProductReference Product { get; }

    void Cancel();
}