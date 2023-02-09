using System;
using System.Linq;
using AnakinRaW.AppUpaterFramework.Conditions;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Product;

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
        return new CompositeConditionsEvaluator(ServiceProvider)
            .EvaluateConditions(component.DetectConditions, productVariables.ToDictionary());
    }
}