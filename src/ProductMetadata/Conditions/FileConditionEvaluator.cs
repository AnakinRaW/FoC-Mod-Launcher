using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Hashing;
using Sklavenwalker.ProductMetadata.Component;
using Sklavenwalker.ProductMetadata.Services;
using Validation;

namespace Sklavenwalker.ProductMetadata.Conditions;

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
            if (!EvaluateFileHash(hashingService, fileSystem.FileInfo.FromFileName(filePath),
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

    private static bool EvaluateFileVersion(string filePath, Version version)
    {
        if (!Version.TryParse(FileVersionInfo.GetVersionInfo(filePath).ProductVersion, out var fileProductVersion) || fileProductVersion is null)
            return false;
        return version.Equals(fileProductVersion);
    }
}