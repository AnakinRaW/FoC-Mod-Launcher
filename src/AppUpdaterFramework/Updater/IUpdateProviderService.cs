﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Metadata.Update;

namespace AnakinRaW.AppUpaterFramework.Updater;

public interface IUpdateProviderService
{
    event EventHandler CheckingForUpdatesStarted;

    event EventHandler<IUpdateCatalog?> CheckingForUpdatesCompleted;

    event EventHandler<IUpdateSession> UpdateStarted;

    event EventHandler UpdateCompleted;

    Task CheckForUpdates(IProductReference productReference, CancellationToken token = default);

    Task<object> Update(IUpdateCatalog updateCatalog, CancellationToken token = default);
}