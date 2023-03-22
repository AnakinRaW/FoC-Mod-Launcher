namespace AnakinRaW.AppUpdaterFramework.Conditions;

public interface ICondition
{
    ConditionType Type { get; }

    string Id { get; }

    ConditionJoin Join { get; }
}