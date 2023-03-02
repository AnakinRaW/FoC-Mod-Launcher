﻿using System.IO.Abstractions;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Utilities;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Metadata.Component;

/// <summary>
/// A component representing a single file.
/// </summary>
/// <remarks>
/// The property <see cref="IInstallableComponent.DetectConditions"/> shall not get used for installation detection.
/// </remarks>
public class SingleFileComponent : InstallableComponent, IPhysicalInstallable
{
    public override ComponentType Type => ComponentType.File;

    /// <inheritdoc/>
    public string InstallPath { get; }

    public string FileName { get; }

    public SingleFileComponent(IProductComponentIdentity identity, string installPath, string fileName, OriginInfo? originInfo) 
        : base(identity, originInfo)
    {
        Requires.NotNullOrEmpty(installPath, nameof(installPath));
        Requires.NotNullOrEmpty(fileName, nameof(fileName));
        InstallPath = installPath;
        FileName = fileName;
    }

    internal IFileInfo GetFile(IFileSystem fileSystem, ProductVariables? variables = null)
    {
        var variablesDict = variables?.ToDictionary();
        var fileName = VariableResolver.Default.ResolveVariables(FileName, variablesDict);
        var installPath = VariableResolver.Default.ResolveVariables(InstallPath, variablesDict);
        return fileSystem.FileInfo.New(fileSystem.Path.Combine(installPath, fileName));
    }
}