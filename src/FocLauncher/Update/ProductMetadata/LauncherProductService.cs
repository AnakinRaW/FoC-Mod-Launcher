using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using FocLauncher.Update.Manifest;
using Microsoft.Extensions.DependencyInjection;
using Semver;
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

    protected override IReadOnlyList<IProductComponent> FindInstalledComponents()
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
    internal const string StableBranchName = "stable";

    private const string BranchLookupFileName = "branches";
    private const string ManifestFileName = "manifest.json";

    private static readonly Url BranchLookupUrl = Url.Combine(LauncherConstants.LauncherRootUrl, BranchLookupFileName);

    protected override string DefaultBranchName => StableBranchName;

    public LauncherBranchManager(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override async Task<IEnumerable<ProductBranch>> GetAvailableBranches()
    {

        var branchesData = await new WebClient().DownloadDataTaskAsync(BranchLookupUrl.ToUri());
        var branchNames = Encoding.UTF8.GetString(branchesData).Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (!branchNames.Any())
            throw new InvalidOperationException($"No branches detected in '{BranchLookupUrl}'");
        var branches = new List<ProductBranch>();
        foreach (var name in branchNames)
        {
            var isPrerelease = !name.Equals(DefaultBranchName, StringComparison.InvariantCultureIgnoreCase);
            branches.Add(new ProductBranch(name, BuildManifestUri(name), isPrerelease));
        }
        return branches;
    }

    protected override Uri BuildManifestUri(string branchName)
    {
        return new Uri(@"C:\Users\Anakin\Desktop\manifest.json", UriKind.Absolute);
        //return LauncherConstants.LauncherRootUrl.AppendPathSegments(branchName, ManifestFileName).ToUri();
    }
}

internal class LauncherManifestLoader : ManifestLoaderBase
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public LauncherManifestLoader(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<IProductCatalog> LoadManifestCore(Stream manifest, IProductReference productReference, CancellationToken cancellationToken)
    {
        var launcherManifest = await JsonSerializer.DeserializeAsync<LauncherManifest>(manifest, JsonSerializerOptions, cancellationToken);
        if (launcherManifest is null)
            throw new CatalogException("Serialized manifest is null");

        var availProduct = BuildReference(launcherManifest);
        ValidateCompatibleManifest(availProduct, productReference);
        var catalog = BuildCatalog(launcherManifest.Components);
        return new ProductCatalog(availProduct, catalog);
    }

    private IProductReference BuildReference(LauncherManifest launcherManifest)
    {
        SemVersion? version = null;
        if (launcherManifest.Version is not null)
            version = SemVersion.Parse(launcherManifest.Version, SemVersionStyles.Any);

        ProductBranch? branch = null;
        if (version is not null && launcherManifest.Branch is not null)
        {
            var branchManager = ServiceProvider.GetRequiredService<IBranchManager>();
            branch = branchManager.GetBranchFromVersion(version);
        }
        return new ProductReference(launcherManifest.Name, version, branch);
    }

    private static IReadOnlyList<IProductComponent> BuildCatalog(IEnumerable<LauncherComponent> launcherManifestComponents)
    {
        var catalog = new List<IProductComponent>();
        foreach (var manifestComponent in launcherManifestComponents)
        {
            switch (manifestComponent.Type)
            {
                case ComponentType.File:
                    catalog.Add(manifestComponent.ToInstallable());
                    break;
                case ComponentType.Group:
                    catalog.Add(manifestComponent.ToGroup());
                    break;
                default:
                    throw new InvalidOperationException($"{manifestComponent.Type} is not supported.");
            }
        }
        return catalog;
    }
}