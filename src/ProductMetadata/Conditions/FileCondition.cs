﻿using System;
using AnakinRaW.ProductMetadata.Component;
using Validation;

namespace AnakinRaW.ProductMetadata.Conditions;

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