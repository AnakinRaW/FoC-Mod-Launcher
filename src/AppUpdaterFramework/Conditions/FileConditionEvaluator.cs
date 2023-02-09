using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.AppUpaterFramework.Utilities;
using AnakinRaW.CommonUtilities.Hashing;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Conditions;

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
        var variableResolver = services.GetService<IVariableResolver>();

        var filePath = fileCondition.FilePath;
        filePath = variableResolver?.ResolveVariables(filePath, properties) ?? filePath;
        if (string.IsNullOrEmpty(filePath) || !fileSystem.File.Exists(filePath))
            return false;
        if (fileCondition.IntegrityInformation.HashType != HashType.None)
        {
            var hashingService = services.GetRequiredService<IHashingService>();
            if (!EvaluateFileHash(hashingService, fileSystem.FileInfo.New(filePath),
                    fileCondition.IntegrityInformation.HashType, fileCondition.IntegrityInformation.Hash))
                return false;
        }

        if (fileCondition.Version != null)
        {
            if (!EvaluateFileVersion(fileSystem, filePath, fileCondition.Version))
                return false;
        }
        return true;
    }

    private static bool EvaluateFileHash(IHashingService hashingService, IFileInfo file, HashType hashType, byte[] expectedHash)
    {
        var actualHash = hashingService.GetFileHash(file, hashType);
        return actualHash.SequenceEqual(expectedHash);
    }

    private static bool EvaluateFileVersion(IFileSystem fileSystem, string filePath, Version version)
    {
        return Version.Parse(FileVersionInfo.GetVersionInfo(filePath).FileVersion).Equals(version);
    }
}