using System;
using System.Collections.Generic;
using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductMetadata.Conditions;

public interface IConditionEvaluator
{
    ConditionType Type { get; }

    bool Evaluate(IServiceProvider services, ICondition condition, IDictionary<string, string?>? properties = null);
}