using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Component;
using Validation;

namespace AnakinRaW.ProductMetadata.Conditions;

internal sealed class ConditionEvaluatorStore : IConditionEvaluatorStore
{
    private readonly IDictionary<ConditionType, IConditionEvaluator> _conditionEvaluators =
        new Dictionary<ConditionType, IConditionEvaluator>();

    public void AddConditionEvaluator(IConditionEvaluator evaluator)
    {
        Requires.NotNull(evaluator, nameof(evaluator));
        _conditionEvaluators[evaluator.Type] = evaluator;
    }

    public IConditionEvaluator? GetConditionEvaluator(ICondition? condition)
    {
        if (condition == null)
            return null;
        _conditionEvaluators.TryGetValue(condition.Type, out var conditionEvaluator);
        return conditionEvaluator;
    }
}