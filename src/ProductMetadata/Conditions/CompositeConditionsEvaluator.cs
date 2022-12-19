using System;
using System.Collections.Generic;
using Validation;

namespace Sklavenwalker.ProductMetadata.Conditions;

public class CompositeConditionsEvaluator
{
    private readonly IServiceProvider _services;
    private readonly IConditionEvaluatorStore _evaluatorStore;


    public CompositeConditionsEvaluator(IServiceProvider services, IConditionEvaluatorStore evaluatorStore)
    {
        Requires.NotNull(evaluatorStore, nameof(evaluatorStore));
        _services = services;
        _evaluatorStore = evaluatorStore;
    }

    public bool EvaluateConditions(IList<ICondition> conditions, IDictionary<string, string?>? properties = null)
    {
        Requires.NotNull(conditions, nameof(conditions));
        
        var result = false;
        var resultList = new List<(bool value, ConditionJoin join)>();
        foreach (var condition in conditions)
        {
            var evaluator = _evaluatorStore.GetConditionEvaluator(condition);
            if (evaluator is null)
                throw new Exception($"Cannot find evaluator for {condition.Id} of type {condition.Type}");
            var evaluation = evaluator.Evaluate(_services, condition, properties);
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