using AnakinRaW.ProductMetadata.Conditions;
using AnakinRaW.ProductMetadata.Services.Detectors;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.ProductMetadata;

public static class LibraryInitialization
{
    public static void AddProductMetadata(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IManifestInstallationDetector>(sp => new ManifestInstallationDetector(sp));

        var conditionEvaluator = new ConditionEvaluatorStore();
        conditionEvaluator.AddConditionEvaluator(new FileConditionEvaluator());
        serviceCollection.AddSingleton<IConditionEvaluatorStore>(_ => conditionEvaluator);
    }
}