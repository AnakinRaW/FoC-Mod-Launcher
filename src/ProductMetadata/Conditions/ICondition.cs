using AnakinRaW.ProductMetadata.Component;

namespace AnakinRaW.ProductMetadata.Conditions;

public interface ICondition
{
    ConditionType Type { get; }

    string Id { get; }

    ConditionJoin Join { get; }
}