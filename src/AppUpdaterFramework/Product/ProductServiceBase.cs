using System;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Product.Manifest;
using AnakinRaW.AppUpdaterFramework.Restart;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Semver;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Product;

public abstract class ProductServiceBase : IProductService
{ 
    private bool _isInitialized;
    private InstalledProduct? _installedProduct;

    private readonly object _syncLock = new();
    private readonly IRestartManager _restartManager;

    public abstract IDirectoryInfo InstallLocation { get; }

    protected ILogger? Logger { get; }

    protected IServiceProvider ServiceProvider { get; }

    protected ProductServiceBase(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _restartManager = serviceProvider.GetRequiredService<IRestartManager>();
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
        lock (_syncLock)
        {
            _installedProduct = BuildProduct();
        }
        _restartManager.RebootRequired += OnRebootRequired;
        _isInitialized = true;
    }

    private void OnRebootRequired(object sender, EventArgs e)
    {
        _installedProduct!.RequiresRestart = true;
        Logger?.LogTrace("Restart required for current instance.");
        _restartManager.RebootRequired -= OnRebootRequired;
    }

    private InstalledProduct BuildProduct()
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