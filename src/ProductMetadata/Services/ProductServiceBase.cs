using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sklavenwalker.CommonUtilities.FileSystem;
using Sklavenwalker.ProductMetadata.Catalog;
using Sklavenwalker.ProductMetadata.Component;
using Sklavenwalker.ProductMetadata.Services.Detectors;
using Validation;

namespace Sklavenwalker.ProductMetadata.Services;

public abstract class ProductServiceBase : IProductService
{
    private readonly IServiceProvider _serviceProvider;
    private bool _isInitialized;
    private IInstalledProduct? _installedProduct;
    private readonly IFileSystemService _fsUtils;


    protected ILogger? Logger;
    protected readonly IFileSystem FileSystem;
        
    protected ICatalogBuilder CatalogBuilder { get; }
        
    protected IManifestFileResolver? ManifestFileResolver { get; private set; }

    protected IComponentDetectorFactory? ComponentDetectorFactory { get; private set; }

    protected ProductServiceBase(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        CatalogBuilder = serviceProvider.GetRequiredService<ICatalogBuilder>();

        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _fsUtils = serviceProvider.GetRequiredService<IFileSystemService>();
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public IInstalledProduct GetCurrentInstance()
    {
        Initialize();
        return _installedProduct!;
    }

    public void UpdateCurrentInstance(IInstalledProduct product)
    {
        throw new NotImplementedException();
    }

    public abstract IProductReference CreateProductReference(Version? newVersion, string? branch);

    public IInstalledProductCatalog GetInstalledProductCatalog()
    {
        Initialize();
        var manifest = _installedProduct!.Manifest;
        var installPath = _installedProduct.InstallationPath;
        return new InstalledProductCatalog(_installedProduct!, FindInstalledComponents(manifest, installPath));
    }

    public IProductCatalog GetAvailableProductManifest(CatalogLocation manifestLocation)
    {
        Initialize();
        if (!IsProductCompatible(manifestLocation.Product))
            throw new InvalidOperationException("Not compatible product");
            
        try
        {
            Logger?.LogTrace("Getting manifest file.");
            var manifestFile = GetAvailableManifestFile(manifestLocation);
            if (manifestFile is null || !manifestFile.Exists)
                throw new CatalogException("Manifest file not found or null");
            try
            {
                Logger?.LogTrace($"Loading manifest form {manifestFile.FullName}");
                var manifest = LoadManifest(manifestFile, manifestLocation.Product);
                if (manifest is null)
                    throw new CatalogException("Manifest cannot be null");
                return manifest;
            }
            finally
            {
                _fsUtils.DeleteFileIfInTemp(manifestFile);
            }
        }
        catch (Exception e)
        {
            Logger?.LogError(e, e.Message);
            throw;
        }
    }

    protected abstract IInstalledProduct BuildProduct();
        
    private IEnumerable<IProductComponent> FindInstalledComponents(IProductCatalog manifest, string installationPath)
    {
        var currentInstance = GetCurrentInstance();
        return manifest.Items.Select(component =>
        {
            var detector = ComponentDetectorFactory!.GetDetector(component.Type, _serviceProvider);
            return detector.Find(component, currentInstance);
        });
    }

    protected virtual IFileInfo GetAvailableManifestFile(CatalogLocation manifestLocation)
    {
        return ManifestFileResolver!.GetManifest(manifestLocation.ManifestUri);
    }
        
    protected virtual bool IsProductCompatible(IProductReference product)
    {
        return !ProductReferenceEqualityComparer.NameOnly.Equals(_installedProduct!, product);
    }
        
    protected virtual IProductCatalog LoadManifest(IFileInfo manifestFile, IProductReference productReference)
    {
        return CatalogBuilder.Build(manifestFile, productReference);
    }

    private void Initialize()
    {
        if (_isInitialized)
            return;
        ManifestFileResolver = _serviceProvider.GetService<IManifestFileResolver>() ?? new LocalManifestFileResolver(_serviceProvider);
        ComponentDetectorFactory = _serviceProvider.GetService<IComponentDetectorFactory>() ?? new ComponentDetectorFactory();
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