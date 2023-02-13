using System;
using AnakinRaW.AppUpaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpaterFramework.Updater;

public interface IUpdateSession
{
    event EventHandler<ProgressEventArgs?> DownloadProgress;

    event EventHandler<ProgressEventArgs?> InstallProgress;

    IProductReference Product { get; }

    void Cancel();
}