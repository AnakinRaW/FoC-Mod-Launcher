using System;
using System.Collections.Generic;

namespace AnakinRaW.AppUpdaterFramework.Conditions;

public interface IConditionEvaluator
{
    ConditionType Type { get; }

    bool Evaluate(IServiceProvider services, ICondition condition, IDictionary<string, string?>? properties = null);
}