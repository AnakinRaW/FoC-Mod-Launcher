﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Updater;

public interface IUpdateSession
{
    event EventHandler<ProgressEventArgs?> DownloadProgress;

    event EventHandler<ProgressEventArgs?> InstallProgress;

    IProductReference Product { get; }

    void Cancel();
}

internal class UpdateSession : IUpdateSession
{
    private readonly IApplicationUpdater _updater;
    private readonly CancellationTokenSource _cts = new();

    public event EventHandler<ProgressEventArgs?>? DownloadProgress;
    public event EventHandler<ProgressEventArgs?>? InstallProgress;
    public IProductReference Product { get; }

    public UpdateSession(IProductReference product, IApplicationUpdater updater)
    {
        Requires.NotNull(product, nameof(product));
        Requires.NotNull(updater, nameof(updater));
        Product = product;
        _updater = updater;
    }

    internal async Task<object> StartUpdate()
    {
        try
        {
            _updater.Progress += OnProgress;
            return await _updater.UpdateAsync(_cts.Token);
        }
        finally
        {
            _updater.Progress -= OnProgress;
        }
    }

    private void OnProgress(object sender, ProgressEventArgs? e)
    {
        if (e is null)
            return;
        switch (e.Type)
        {
            case ProgressType.Install:
                InstallProgress?.Invoke(this, e);
                break;
            case ProgressType.Download:
            case ProgressType.Verify:
                DownloadProgress?.Invoke(this, e);
                break;
        }
    }

    public void Cancel()
    {
        _cts.Cancel();
    }
}