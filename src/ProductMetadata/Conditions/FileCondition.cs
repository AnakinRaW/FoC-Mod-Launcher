using AnakinRaW.ProductMetadata.Component;
using Semver;
using Validation;

namespace AnakinRaW.ProductMetadata.Conditions;

public sealed record FileCondition : ICondition
{
    public ConditionType Type => ConditionType.File;

    public string Id => "FileCondition";

    public string FilePath { get; }

    public ConditionJoin Join { get; init; }

    public ComponentIntegrityInformation IntegrityInformation { get; init; }

    public SemVersion? Version { get; init; }
    
    public FileCondition(string filePath)
    {
        Requires.NotNullOrEmpty(filePath, nameof(filePath));
        FilePath = filePath;
    }
}