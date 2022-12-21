using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sklavenwalker.ProductMetadata.Component;
using Sklavenwalker.ProductMetadata.Conditions;

namespace Sklavenwalker.ProductMetadata.Services.Detectors;

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
            return new CompositeConditionsEvaluator(ServiceProvider).EvaluateConditions(component.DetectConditions, productVariables.ToDictionary());
        }
        catch (Exception e)
        {
            Logger?.LogWarning($"Error evaluating detection condition for {component.Id}. Error: {e}");
            return false;
        }
    }
}