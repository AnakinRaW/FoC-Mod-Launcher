using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reflection;
using System.Threading.Tasks;
using Flurl;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.ProductMetadata;
using Sklavenwalker.ProductMetadata.Catalog;
using Sklavenwalker.ProductMetadata.Component;
using Sklavenwalker.ProductMetadata.Services;

namespace FocLauncher.Update.ProductMetadata;

internal class LauncherProductService : ProductServiceBase
{
    private readonly IFileSystem _fileSystem;
    private readonly IBranchManager _branchManager;

    public LauncherProductService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _branchManager = serviceProvider.GetRequiredService<IBranchManager>();
    }

    protected override IInstalledProduct BuildProduct()
    {
        var productRef = CreateCurrentProductReference();
        var installLocation = _fileSystem.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return new InstalledProduct(productRef, new ProductCatalog(productRef, Array.Empty<IProductComponent>()), installLocation);
    }

    protected override IEnumerable<IProductComponent> FindInstalledComponents()
    {
        throw new NotImplementedException();
    }

    private IProductReference CreateCurrentProductReference()
    {
        const string name = LauncherConstants.ApplicationName;
        var version = LauncherAssemblyInfo.InformationalAsSemVer();
        ProductBranch? branch = null;
        if (version is not null)
            branch = _branchManager.GetBranchFromVersion(version);
        return new ProductReference(name, version, branch);
    }
}

public class LauncherBranchManager : BranchManager
{
    private const string BranchLookupFileName = "branches";
    private const string ManifestFileName = "manifest.json";

    private static readonly Url BranchLookupUrl = new Url(LauncherConstants.LauncherRootUrl).AppendPathSegment(BranchLookupFileName);
    
    protected override string DefaultBranchName => "stable";

    public LauncherBranchManager(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task<IEnumerable<ProductBranch>> GetAvailableBranches()
    {
        throw new NotImplementedException();
    }

    protected override async Task<IProductCatalog> LoadManifest(IFileInfo manifestFile, IProductReference productReference)
    {
        return null;
    }

    protected override Uri BuildManifestUri(string branchName)
    {
        return LauncherConstants.LauncherRootUrl.AppendPathSegments(branchName, ManifestFileName).ToUri();
    }


    //private string TemporaryDownloadDirectory
    //{
    //    get
    //    {
    //        if (string.IsNullOrWhiteSpace(_temporaryDownloadDirectory) || !_fileSystem.Directory.Exists(_temporaryDownloadDirectory))
    //            _temporaryDownloadDirectory = _fileSystemHelper.CreateTemporaryFolderInTempWithRetry(10)?.FullName ??
    //                                          throw new IOException("Unable to create temporary directory");
    //        return _temporaryDownloadDirectory!;
    //    }
    //}

    //private string CreateRandomFile()
    //{
    //    var location = TemporaryDownloadDirectory;
    //    string file;
    //    var count = 0;
    //    do
    //    {
    //        var fileName = _fileSystem.Path.GetRandomFileName();
    //        file = _fileSystem.Path.Combine(location, fileName);
    //    } while (_fileSystem.File.Exists(file) && count++ <= 10);
    //    if (count < 10)
    //        throw new IOException($"unable to create temporary file under '{location}'");
    //    return file;
    //}
}