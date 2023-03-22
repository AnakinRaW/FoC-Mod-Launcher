using System;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Elevation;
using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Product.Manifest;
using AnakinRaW.AppUpdaterFramework.Restart;
using AnakinRaW.AppUpdaterFramework.Updater;
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
    private readonly IElevationManager _elevationManager;

    public abstract IDirectoryInfo InstallLocation { get; }

    protected ILogger? Logger { get; }

    protected IServiceProvider ServiceProvider { get; }

    protected ProductServiceBase(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _restartManager = serviceProvider.GetRequiredService<IRestartManager>();
        _restartManager.RebootRequired += OnRebootRequired!;
        _elevationManager = serviceProvider.GetRequiredService<IElevationManager>();
        _elevationManager.ElevationRequested += OnElevationRequested;
        serviceProvider.GetRequiredService<IUpdateService>().UpdateCompleted += OnUpdateCompleted!;
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

    private ProductState FetchInstallState()
    {
        var state = ProductState.Installed;

        if (_restartManager.RequiredRestartType == RestartType.ApplicationRestart)
            state |= ProductState.RestartRequired;
        if (_elevationManager.IsElevationRequested)
            state |= ProductState.ElevationRequired;
        return state;
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
            var installedProduct = BuildProduct();
            if (installedProduct is null)
            {
                var ex = new InvalidOperationException("Created Product must not be null!");
                Logger?.LogError(ex, ex.Message);
                throw ex;
            }
            _installedProduct = installedProduct;
        }
    }

    private InstalledProduct BuildProduct()
    {
        var productReference = CreateCurrentProductReference();
        var variables = AddProductVariables(productReference);
        var manifest = ServiceProvider.GetRequiredService<IInstalledManifestProvider>().ProvideManifest(productReference, variables);
        var state = FetchInstallState();
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

    private void OnRebootRequired(object sender, EventArgs e)
    {
        try
        {
            if (_installedProduct is null)
                return;
            _installedProduct!.State |= ProductState.RestartRequired;
            Logger?.LogTrace("Restart required for current instance.");
        }
        finally
        {
            _restartManager.RebootRequired -= OnRebootRequired;
        }
    }

    private void OnElevationRequested(object? sender, EventArgs e)
    {
        try
        {
            if (_installedProduct is null)
                return;
            _installedProduct!.State |= ProductState.ElevationRequired;
            Logger?.LogTrace("Elevation required for current instance.");
        }
        finally
        {
            _elevationManager.ElevationRequested -= OnElevationRequested;
        }
    }

    private void OnUpdateCompleted(object sender, EventArgs e)
    {
        Reset();
    }
}