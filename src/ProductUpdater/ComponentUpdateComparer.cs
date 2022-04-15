using System;
using System.Collections.Generic;
using Sklavenwalker.ProductMetadata.Component;
using Sklavenwalker.ProductMetadata.Conditions;
using Sklavenwalker.ProductUpdater.Catalog;

namespace Sklavenwalker.ProductUpdater;

public class ComponentUpdateComparer : IComponentComparer
{
    private readonly IServiceProvider _serviceProvider;

    private ConditionEvaluatorFactory? _evaluatorFactory;

    private ConditionEvaluatorFactory EvaluatorFactory
    {
        get
        {
            if (_evaluatorFactory is null)
            {
                var factory = new ConditionEvaluatorFactory();
                factory.AddConditionEvaluator(new FileConditionEvaluator());
                _evaluatorFactory = factory;
            }
            return _evaluatorFactory;
        }
    }

    public ComponentUpdateComparer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public UpdateAction Compare(
        IInstallableComponent? installedComponent, 
        IInstallableComponent? availableComponent,
        IDictionary<string, string?>? properties = null)
    {
        if (installedComponent is null && availableComponent is null)
            throw new InvalidOperationException("Unable to compare null components");
        if (installedComponent is null)
            return UpdateAction.Update;
        if (availableComponent is null)
            return UpdateAction.Delete;
        if (ReferenceEquals(installedComponent, availableComponent))
            return 0;
        
        if (!ProductComponentIdentityComparer.VersionAndBranchIndependent.Equals(installedComponent, availableComponent))
            throw new InvalidOperationException("Current and available components are not compatible.");

        if (installedComponent.DetectedState != DetectionState.Present)
            return UpdateAction.Update;
        if (availableComponent.DetectConditions.Count == 0)
            return 0;

        return !new CompositeConditionsEvaluator().EvaluateConditions(_serviceProvider,
            availableComponent.DetectConditions, EvaluatorFactory, properties) ? UpdateAction.Update : UpdateAction.Keep;
    }
}