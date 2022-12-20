﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.ProductMetadata.Component;
using Sklavenwalker.ProductMetadata.Conditions;
using Sklavenwalker.ProductUpdater.Catalog;

namespace Sklavenwalker.ProductUpdater.Services;

public class ComponentUpdateComparer : IComponentComparer
{
    private readonly CompositeConditionsEvaluator _evaluator;

    public ComponentUpdateComparer(IServiceProvider serviceProvider)
    {
        var evaluatorStore = serviceProvider.GetRequiredService<IConditionEvaluatorStore>();
        _evaluator = new CompositeConditionsEvaluator(serviceProvider, evaluatorStore);
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

        return !_evaluator.EvaluateConditions(availableComponent.DetectConditions, properties)
            ? UpdateAction.Update
            : UpdateAction.Keep;
    }
}