﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Product.Manifest;
using AnakinRaW.CommonUtilities.DownloadManager;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Semver;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Product;

public abstract class BranchManager : IBranchManager
{
    private readonly ILogger? _logger;
    private readonly IFileSystemService _fileSystemHelper;
    private readonly IManifestDownloader _manifestDownloader;

    protected readonly IManifestLoader ManifestLoader;

    protected abstract string DefaultBranchName { get; }

    protected BranchManager(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _fileSystemHelper = serviceProvider.GetRequiredService<IFileSystemService>();
        _manifestDownloader = serviceProvider.GetService<IManifestDownloader>() ?? new ManifestDownloader(serviceProvider);
        ManifestLoader = serviceProvider.GetRequiredService<IManifestLoader>();
    }

    public abstract Task<IEnumerable<ProductBranch>> GetAvailableBranches();

    public virtual ProductBranch GetBranchFromVersion(SemVersion version)
    {
        var branchName = version.Prerelease;
        if (string.IsNullOrEmpty(branchName))
            branchName = DefaultBranchName;
        var manifestUri = BuildManifestUri(branchName);
        return new ProductBranch(branchName, manifestUri, version.IsPrerelease);
    }

    public async Task<IProductManifest> GetManifest(IProductReference productReference, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        Requires.NotNull(productReference, nameof(productReference));
        if (productReference.Branch is null)
            throw new InvalidOperationException("No branch specified.");

        var branch = productReference.Branch;
        ValidateBranchUri(branch.ManifestLocation);

        try
        {
            var manifestFile = await _manifestDownloader.GetManifest(branch.ManifestLocation, token);
            try
            {
                var manifest = await ManifestLoader.LoadManifest(manifestFile, productReference, token);
                return manifest ?? throw new CatalogException("No catalog was created");
            }
            finally
            {
                try
                {
                    _fileSystemHelper.DeleteFileIfInTemp(manifestFile);
                }
                catch (Exception e)
                {
                    _logger?.LogWarning($"Failed to delete manifest {e.Message}");
                }
            }
        }
        catch (CatalogException ex)
        {
            _logger?.LogError(ex, ex.Message);
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DownloadFailedException ex)
        {
            var message = $"Could not download branch manifest from '{branch.ManifestLocation.AbsoluteUri}'";
            _logger?.LogError(ex, message);
            throw new CatalogDownloadException(message, ex);
        }
        catch (Exception ex)
        {
            var message = $"Unable to get manifest of branch:' {branch.Name}'";
            _logger?.LogError(ex, message);
            throw new CatalogException(message, ex);
        }
    }
    
    protected abstract Uri BuildManifestUri(string branchName);

    private void ValidateBranchUri(Uri branchManifestLocation)
    {
        if (branchManifestLocation is null)
        {
            var ex = new InvalidOperationException("The branch's manifest location is null.");
            _logger?.LogError(ex, ex.Message);
            throw ex;
        }
        if (!branchManifestLocation.IsAbsoluteUri)
        {
            var ex = new InvalidOperationException($"The branch's manifest location: '{branchManifestLocation.AbsoluteUri}' needs to be an absolute uri.");
            _logger?.LogError(ex, ex.Message);
            throw ex;
        }
    }
}