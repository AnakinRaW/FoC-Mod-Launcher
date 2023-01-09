using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.ProductMetadata.Catalog;
using AnakinRaW.ProductMetadata.Component;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Semver;
using Validation;

namespace AnakinRaW.ProductMetadata.Services;

public abstract class ProductServiceBase : IProductService
{
    private readonly IServiceProvider _serviceProvider;
    private bool _isInitialized;
    private IInstalledProduct? _installedProduct;
    protected ILogger? Logger;
    protected readonly IFileSystem FileSystem;

    protected ProductServiceBase(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public IInstalledProduct GetCurrentInstance()
    {
        Initialize();
        return _installedProduct!;
    }

    public virtual IProductReference CreateProductReference(SemVersion? newVersion, ProductBranch? newBranch)
    {
        var current = GetCurrentInstance();
        var branch = current.Branch;
        if (newBranch is not null)
            branch = newBranch;
        var version = current.Version;
        if (newVersion is not null)
            version = newVersion;
        return new ProductReference(current.Name, version, branch);
    }

    public IInstalledProductCatalog GetInstalledProductCatalog()
    {
        Initialize();
        return new InstalledProductCatalog(_installedProduct!, FindInstalledComponents());
    }
    
    protected abstract IInstalledProduct BuildProduct();

    protected abstract IReadOnlyList<IProductComponent> FindInstalledComponents();

    public virtual bool IsProductCompatible(IProductReference product)
    {
        return !ProductReferenceEqualityComparer.NameOnly.Equals(_installedProduct!, product);
    }

    private void Initialize()
    {
        if (_isInitialized)
            return;
        _installedProduct ??= BuildProduct();
        AddProductVariables(_installedProduct);
        if (_installedProduct is null)
            throw new InvalidOperationException("Created Product must not be null!");
        _isInitialized = true;
    }

    private void AddProductVariables(IInstalledProduct product)
    {
        var fs = _serviceProvider.GetService<IFileSystem>() ?? new FileSystem();
        product.ProductVariables.Add(KnownProductVariablesKeys.InstallDir, product.InstallationPath);
        product.ProductVariables.Add(KnownProductVariablesKeys.InstallDrive, fs.Path.GetPathRoot(product.InstallationPath));
        AddAdditionalProductVariables(product);
    }

    protected virtual void AddAdditionalProductVariables(IInstalledProduct product)
    {
    }
}