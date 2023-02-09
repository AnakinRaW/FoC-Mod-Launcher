using AnakinRaW.AppUpaterFramework.Conditions;
using AnakinRaW.AppUpaterFramework.Product.Manifest;
using AnakinRaW.AppUpaterFramework.Updater;
using AnakinRaW.AppUpaterFramework.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.AppUpaterFramework;

public static class LibraryInitialization
{
    public static void AddUpdateFramework(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IVariableResolver>(_ => new VariableResolver());
        serviceCollection.AddSingleton<IUpdateProviderService>(sp => new UpdateProviderService(sp));
        serviceCollection.AddSingleton<IUpdateCatalogProvider>(sp => new UpdateCatalogProvider(sp));

        serviceCollection.AddSingleton<IManifestInstallationDetector>(sp => new ManifestInstallationDetector(sp));

        var conditionEvaluator = new ConditionEvaluatorStore();
        conditionEvaluator.AddConditionEvaluator(new FileConditionEvaluator());
        serviceCollection.AddSingleton<IConditionEvaluatorStore>(_ => conditionEvaluator);
    }
}