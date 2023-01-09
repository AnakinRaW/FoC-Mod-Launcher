using System;
using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Component;

namespace AnakinRaW.ProductMetadata.Conditions;

public interface IConditionEvaluator
{
    ConditionType Type { get; }

    bool Evaluate(IServiceProvider services, ICondition condition, IDictionary<string, string?>? properties = null);
}