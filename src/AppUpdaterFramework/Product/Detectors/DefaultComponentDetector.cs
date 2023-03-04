using System;
using System.Linq;
using AnakinRaW.AppUpdaterFramework.Conditions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpdaterFramework.Product.Detectors;

internal sealed class DefaultComponentDetector : ComponentDetectorBase<InstallableComponent>
{
    public DefaultComponentDetector(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override bool FindCore(InstallableComponent component, ProductVariables productVariables)
    {
        if (!component.DetectConditions.Any())
            return true;
        return new CompositeConditionsEvaluator(ServiceProvider)
            .EvaluateConditions(component.DetectConditions, productVariables.ToDictionary());
    }
}