﻿namespace AnakinRaW.ProductMetadata.Conditions;

public interface IConditionEvaluatorStore
{
    void AddConditionEvaluator(IConditionEvaluator evaluator);

    IConditionEvaluator? GetConditionEvaluator(ICondition? condition);
}