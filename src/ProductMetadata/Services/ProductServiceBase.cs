using System;
using System.IO.Abstractions;
using AnakinRaW.ProductMetadata.Catalog;
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

    private readonly object _syncLock = new();
   
    protected ILogger? Logger { get; }

    protected ICurrentInstallation CurrentInstallation { get; }

    protected ProductServiceBase(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        CurrentInstallation = serviceProvider.GetRequiredService<ICurrentInstallation>();
    }

    public IInstalledProduct GetCurrentInstance()
    {
        Initialize();
        lock (_syncLock)
        {
            return _installedProduct!;
        }
    }

    public IInstalledProductCatalog GetInstalledProductCatalog()
    {
        Initialize();
        return new InstalledProductCatalog(GetCurrentInstance(), CurrentInstallation.FindInstalledComponents());
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

    public virtual bool IsProductCompatible(IProductReference product)
    {
        var installed = GetCurrentInstance();
        return !ProductReferenceEqualityComparer.NameOnly.Equals(installed, product);
    }

    public void RevalidateInstallation()
    {
        Logger?.LogDebug("Requested revalidation of current installation.");
        Reset();
    }

    protected abstract IInstalledProduct BuildProduct();
    
    protected virtual void AddAdditionalProductVariables(IInstalledProduct product)
    {
    }

    private void Initialize()
    {
        if (_isInitialized)
            return;
        Reset();
        _isInitialized = true;
    }

    private void Reset()
    {
        lock (_syncLock)
        {
            var installedProduct = BuildProduct();
            if (installedProduct is null)
            {
                var ex = new InvalidOperationException("Created Product must not be null!");
                Logger?.LogError(ex, ex.Message);
                throw ex;
            }
            AddProductVariables(installedProduct);
            _installedProduct = installedProduct;
        }
    }

    private void AddProductVariables(IInstalledProduct product)
    {
        var fs = _serviceProvider.GetService<IFileSystem>() ?? new FileSystem();
        product.ProductVariables.Add(KnownProductVariablesKeys.InstallDir, product.InstallationPath);
        product.ProductVariables.Add(KnownProductVariablesKeys.InstallDrive, fs.Path.GetPathRoot(product.InstallationPath));
        AddAdditionalProductVariables(product);
    }
}