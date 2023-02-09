using System;
using System.Linq;
using AnakinRaW.AppUpaterFramework.Conditions;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using Microsoft.Extensions.Logging;

namespace AnakinRaW.AppUpaterFramework.Product.Detectors;

internal sealed class DefaultComponentDetector : ComponentDetectorBase<InstallableComponent>
{
    public DefaultComponentDetector(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override bool FindCore(InstallableComponent component, ProductVariables productVariables)
    {
        if (!component.DetectConditions.Any())
            return true;
        try
        {
            return new CompositeConditionsEvaluator(ServiceProvider)
                .EvaluateConditions(component.DetectConditions, productVariables.ToDictionary());
        }
        catch (Exception e)
        {
            Logger?.LogWarning($"Error evaluating detection condition for {component.Id}. Error: {e}");
            return false;
        }
    }
}