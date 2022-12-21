using Semver;
using Sklavenwalker.ProductMetadata.Component;
using Validation;

namespace Sklavenwalker.ProductMetadata.Conditions;

public sealed record FileCondition : ICondition
{
    public ConditionType Type => ConditionType.File;

    public string Id => "FileCondition";

    public string FilePath { get; }

    public ConditionJoin Join { get; init; }

    public ComponentIntegrityInformation IntegrityInformation { get; init; }

    public SemVersion? Version { get; init; }
    
    public FileCondition(string file)
    {
        Requires.NotNullOrEmpty(file, nameof(file));
        FilePath = file;
    }
}