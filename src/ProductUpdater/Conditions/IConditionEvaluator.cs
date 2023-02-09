using System;
using System.Collections.Generic;

namespace AnakinRaW.AppUpaterFramework.Conditions;

public interface IConditionEvaluator
{
    ConditionType Type { get; }

    bool Evaluate(IServiceProvider services, ICondition condition, IDictionary<string, string?>? properties = null);
}