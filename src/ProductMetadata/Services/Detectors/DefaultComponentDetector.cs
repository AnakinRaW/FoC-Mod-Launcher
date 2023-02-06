using System;
using System.Linq;
using AnakinRaW.ProductMetadata.Component;
using AnakinRaW.ProductMetadata.Conditions;
using Microsoft.Extensions.Logging;

namespace AnakinRaW.ProductMetadata.Services.Detectors;

internal sealed class DefaultComponentDetector : ComponentDetectorBase<InstallableComponent>
{
    public DefaultComponentDetector(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override bool FindCore(InstallableComponent component, VariableCollection productVariables)
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