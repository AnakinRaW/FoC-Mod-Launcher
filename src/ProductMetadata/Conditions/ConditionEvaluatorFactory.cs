using System.Collections.Generic;
using Sklavenwalker.ProductMetadata.Component;
using Validation;

namespace Sklavenwalker.ProductMetadata.Conditions;

public class ConditionEvaluatorFactory
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