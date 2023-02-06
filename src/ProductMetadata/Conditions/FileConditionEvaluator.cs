using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.ProductMetadata.Component;
using AnakinRaW.ProductMetadata.Services;
using Microsoft.Extensions.DependencyInjection;
using Semver;
using Validation;

namespace AnakinRaW.ProductMetadata.Conditions;

public class FileConditionEvaluator : IConditionEvaluator
{
    public ConditionType Type => ConditionType.File;

    public bool Evaluate(IServiceProvider services, ICondition condition, IDictionary<string, string?>? properties = null)
    {
        Requires.NotNull(services, nameof(services));
        Requires.NotNull(condition, nameof(condition));
        if (condition is not FileCondition fileCondition)
            throw new ArgumentException("condition is not FileCondition", nameof(condition));

        var fileSystem = services.GetRequiredService<IFileSystem>();
        var hashingService = services.GetRequiredService<IHashingService>();
        var variableResolver = services.GetService<IVariableResolver>();

        var filePath = fileCondition.FilePath;
        filePath = variableResolver?.ResolveVariables(filePath, properties) ?? filePath;
        if (string.IsNullOrEmpty(filePath) || !fileSystem.File.Exists(filePath))
            return false;
        if (fileCondition.IntegrityInformation.HashType != HashType.None)
        {
            if (!EvaluateFileHash(hashingService, fileSystem.FileInfo.New(filePath),
                    fileCondition.IntegrityInformation.HashType, fileCondition.IntegrityInformation.Hash))
                return false;
        }

        if (fileCondition.Version != null)
        {
            if (!EvaluateFileVersion(filePath, fileCondition.Version))
                return false;
        }
        return true;
    }

    private static bool EvaluateFileHash(IHashingService hashingService, IFileInfo file, HashType hashType, byte[] expectedHash)
    {
        var actualHash = hashingService.GetFileHash(file, hashType);
        return actualHash.SequenceEqual(expectedHash);
    }

    private static bool EvaluateFileVersion(string filePath, SemVersion version)
    {
        return SemVersion.TryParse(FileVersionInfo.GetVersionInfo(filePath).FileVersion, SemVersionStyles.Any, out var fileProductVersion) 
               && version.Equals(fileProductVersion);
    }
}