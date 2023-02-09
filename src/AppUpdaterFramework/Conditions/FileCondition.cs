using System;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Conditions;

public sealed record FileCondition : ICondition
{
    public ConditionType Type => ConditionType.File;

    public string Id => "FileCondition";

    public string FilePath { get; }

    public ConditionJoin Join { get; init; }

    public ComponentIntegrityInformation IntegrityInformation { get; init; }

    public Version? Version { get; init; }
    
    public FileCondition(string filePath)
    {
        Requires.NotNullOrEmpty(filePath, nameof(filePath));
        FilePath = filePath;
    }
}