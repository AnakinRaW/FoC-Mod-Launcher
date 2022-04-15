using System;
using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductMetadata.Conditions;

public class FileCondition : ICondition
{
    public ConditionType Type => ConditionType.File;

    public string Id { get; }

    public ConditionJoin Join { get; }

    public string FilePath { get; }

    public ComponentIntegrityInformation IntegrityInformation { get; }

    public Version? Version { get; }
    
    public FileCondition(string file)
    {
    }
}