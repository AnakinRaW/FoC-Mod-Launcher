using System;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Updater.Progress;

namespace AnakinRaW.AppUpaterFramework.Updater;

public interface IUpdateSession
{
    event EventHandler<ProgressEventArgs?> DownloadProgress;

    event EventHandler<ProgressEventArgs?> InstallProgress;

    IProductReference Product { get; }

    void Cancel();
}