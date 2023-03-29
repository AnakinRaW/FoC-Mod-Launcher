using System;
using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Conditions;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Component;

public interface IInstallableComponent : IProductComponent
{
    long DownloadSize { get; }

    OriginInfo? OriginInfo { get; }

    IReadOnlyList<ICondition> DetectConditions { get; }

    InstallationSize InstallationSize { get; }

    string? GetFullPath(IServiceProvider serviceProvider, ProductVariables? variables = null);
}