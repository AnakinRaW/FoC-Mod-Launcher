using System;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Product.Manifest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Semver;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Product;

public abstract class ProductServiceBase : IProductService
{ 
    private bool _isInitialized;
    private IInstalledProduct? _installedProduct;

    private readonly object _syncLock = new();

    public abstract IDirectoryInfo InstallLocation { get; }

    protected ILogger? Logger { get; }

    protected IServiceProvider ServiceProvider { get; }

    protected ProductServiceBase(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public IInstalledProduct GetCurrentInstance()
    {
        Initialize();
        lock (_syncLock)
        {
            return _installedProduct!;
        }
    }

    public IInstalledComponentsCatalog GetInstalledComponents()
    {
        Initialize();
        var currentInstance = GetCurrentInstance();
        var detectionService = ServiceProvider.GetRequiredService<IManifestInstallationDetector>();
        var installedComponents = detectionService.DetectInstalledComponents(currentInstance.Manifest, currentInstance.Variables);
        return new InstalledComponentsCatalog(currentInstance, installedComponents);
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
    
    protected abstract IProductReference CreateCurrentProductReference();

    protected virtual ProductInstallState FetchInstallState(IProductReference productReference)
    {
        return ProductInstallState.Installed;
    }


    protected virtual void AddAdditionalProductVariables(ProductVariables variables, IProductReference product)
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
            _installedProduct = BuildProduct();
        }
    }

    private IInstalledProduct BuildProduct()
    {
        var productReference = CreateCurrentProductReference();
        var variables = AddProductVariables(productReference);
        var manifest = ServiceProvider.GetRequiredService<IInstalledManifestProvider>().ProvideManifest(productReference, variables);
        var state = FetchInstallState(productReference);
        return new InstalledProduct(productReference, InstallLocation.FullName, manifest, variables, state);
    }

    private ProductVariables AddProductVariables(IProductReference product)
    {
        var variables = new ProductVariables();
        var installLocation = InstallLocation;
        variables.Add(KnownProductVariablesKeys.InstallDir, installLocation.FullName);
        variables.Add(KnownProductVariablesKeys.InstallDrive, installLocation.Root.FullName);
        AddAdditionalProductVariables(variables, product);
        return variables;
    }
}