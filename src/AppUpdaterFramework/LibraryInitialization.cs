using AnakinRaW.AppUpaterFramework.Conditions;
using AnakinRaW.AppUpaterFramework.Installer;
using AnakinRaW.AppUpaterFramework.Interaction;
using AnakinRaW.AppUpaterFramework.Product.Manifest;
using AnakinRaW.AppUpaterFramework.Updater;
using AnakinRaW.AppUpaterFramework.Updater.Backup;
using AnakinRaW.AppUpaterFramework.Utilities;
using AnakinRaW.CommonUtilities.Hashing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnakinRaW.AppUpaterFramework;

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
    }
}