using System;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Updater.Progress;

namespace AnakinRaW.AppUpdaterFramework.Updater;

public interface IUpdateSession
{
    event EventHandler<ComponentProgressEventArgs> DownloadProgress;

    event EventHandler<ComponentProgressEventArgs> InstallProgress;

    IProductReference Product { get; }

    void Cancel();
}