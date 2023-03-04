namespace AnakinRaW.AppUpdaterFramework.Conditions;

public interface IConditionEvaluatorStore
{
    void AddConditionEvaluator(IConditionEvaluator evaluator);

    IConditionEvaluator? GetConditionEvaluator(ICondition? condition);
}