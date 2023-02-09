using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Conditions;

public class CompositeConditionsEvaluator
{
    private readonly IServiceProvider _services;
    private readonly IConditionEvaluatorStore _evaluatorStore;


    public CompositeConditionsEvaluator(IServiceProvider services)
    {
        _services = services;
        _evaluatorStore = services.GetRequiredService<IConditionEvaluatorStore>();
    }
    
    public bool EvaluateConditions(IReadOnlyList<ICondition> conditions, IDictionary<string, string?>? properties = null)
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