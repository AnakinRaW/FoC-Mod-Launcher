﻿using AnakinRaW.AppUpdaterFramework.Conditions;
using AnakinRaW.AppUpdaterFramework.Elevation;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater;
using AnakinRaW.AppUpdaterFramework.FileLocking;
using AnakinRaW.AppUpdaterFramework.Installer;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.AppUpdaterFramework.Product.Manifest;
using AnakinRaW.AppUpdaterFramework.Restart;
using AnakinRaW.AppUpdaterFramework.Storage;
using AnakinRaW.AppUpdaterFramework.Updater;
using AnakinRaW.AppUpdaterFramework.Utilities;
using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.ExternalUpdater.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnakinRaW.AppUpdaterFramework;

public static class LibraryInitialization
{
    public static void AddUpdateFramework(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IVariableResolver>(_ => new VariableResolver());
        serviceCollection.AddSingleton<IUpdateService>(sp => new UpdateService(sp));
        serviceCollection.AddSingleton<IUpdateCatalogProvider>(sp => new UpdateCatalogProvider(sp));
        
        serviceCollection.AddSingleton<IHashingService>(_ => new HashingService());

        serviceCollection.AddSingleton<IManifestInstallationDetector>(sp => new ManifestInstallationDetector(sp));

        var conditionEvaluator = new ConditionEvaluatorStore();
        conditionEvaluator.AddConditionEvaluator(new FileConditionEvaluator());
        serviceCollection.AddSingleton<IConditionEvaluatorStore>(_ => conditionEvaluator);

        serviceCollection.AddSingleton<IInstallerFactory>(sp => new InstallerFactory(sp));

        serviceCollection.AddSingleton<IDiskSpaceCalculator>(sp => new DiskSpaceCalculator(sp));

        serviceCollection.AddSingleton<IBackupManager>(sp => new BackupManager(sp));

        serviceCollection.AddSingleton<ILockedFileHandler>(sp => new LockedFileHandler(sp));

        serviceCollection.AddSingleton<IInteractionHandler>(sp => new DefaultInteractionHandler(sp));

        serviceCollection.AddSingleton<ILockingProcessManagerFactory>(_ => new LockingProcessManagerFactory());

        serviceCollection.AddSingleton<IRestartManager>(_ => new RestartManager());

        serviceCollection.AddSingleton<IProcessElevation>(_ => new ProcessElevation());

        serviceCollection.AddSingleton(sp => new DownloadRepository(sp));
        serviceCollection.AddSingleton(sp => new BackupRepository(sp));

        serviceCollection.AddSingleton<IWritablePendingComponentStore>(sp => new PendingComponentStore(sp));
        serviceCollection.AddSingleton<IPendingComponentStore>(sp => sp.GetRequiredService<IWritablePendingComponentStore>());

        serviceCollection.AddSingleton<IExternalUpdaterService>(sp => new ExternalUpdaterService(sp));
        serviceCollection.TryAddSingleton<IExternalUpdaterLauncher>(sp => new ExternalUpdaterLauncher(sp));
        serviceCollection.TryAddSingleton<IRegistryExternalUpdaterLauncher>(sp => new RegistryExternalUpdaterLauncher(sp));
    }
}