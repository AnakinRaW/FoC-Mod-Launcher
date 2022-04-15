using System;
using System.Collections.Generic;
using Validation;

namespace Sklavenwalker.ProductMetadata.Conditions;

public class CompositeConditionsEvaluator
{
    public bool EvaluateConditions(
        IServiceProvider services,
        IList<ICondition> conditions,
        ConditionEvaluatorFactory evaluatorFactory,
        IDictionary<string, string?>? properties = null)
    {
        Requires.NotNull(conditions, nameof(conditions));
        Requires.NotNull(evaluatorFactory, nameof(evaluatorFactory));
        var result = false;
        var resultList = new List<(bool value, ConditionJoin join)>();
        foreach (var condition in conditions)
        {
            var evaluator = evaluatorFactory.GetConditionEvaluator(condition);
            if (evaluator is null)
                throw new Exception($"Cannot find evaluator for {condition.Id} of type {condition.Type}");
            var evaluation = evaluator.Evaluate(services, condition, properties);
            resultList.Add((evaluation, condition.Join));
        }

        var flag = ConditionJoin.Or;
        foreach (var (value, join) in resultList)
        {
            if (flag == ConditionJoin.Or)
                result = result || value;
            else
                result = result && value;
            flag = join;
        }

        return result;
    }
}