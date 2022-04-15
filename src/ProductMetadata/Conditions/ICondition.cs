using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductMetadata.Conditions;

public interface ICondition
{
    ConditionType Type { get; }

    string Id { get; }

    ConditionJoin Join { get; }
}