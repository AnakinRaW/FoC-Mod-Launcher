using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;
using Semver;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Product;

public sealed class InstalledProduct : IInstalledProduct
{
    private readonly IProductReference _reference;

    public string Name => _reference.Name;

    public SemVersion? Version => _reference.Version;

    public ProductBranch? Branch => _reference.Branch;

    public string InstallationPath { get; }

    public ProductVariables Variables { get; }

    public ProductState State { get; internal set; }

    public IProductManifest Manifest { get; }

    public InstalledProduct(IProductReference reference, string installationPath, IProductManifest manifest, ProductVariables? variables, ProductState state = ProductState.Installed)
    {
        Requires.NotNull(reference, nameof(reference));
        Requires.NotNull(manifest, nameof(manifest));
        Requires.NotNullOrEmpty(installationPath, nameof(installationPath));
        _reference = reference;
        InstallationPath = installationPath;
        State = state;
        Manifest = manifest;
        Variables = variables ?? new ProductVariables();
    }

    public override string ToString()
    {
        return $"Product '{_reference.Name}, v:{_reference.Version?.ToString() ?? "NO_VERSION"}, Branch:{_reference.Branch?.ToString() ?? "NO_BRANCH"}' " +
               $"at {InstallationPath}";
    }
}