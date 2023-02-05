using AnakinRaW.ProductMetadata.Catalog;
using Semver;
using Validation;

namespace AnakinRaW.ProductMetadata;

public sealed class InstalledProduct : IInstalledProduct
{
    private readonly IProductReference _reference;

    public string Name => _reference.Name;
    public SemVersion? Version => _reference.Version;
    public ProductBranch? Branch => _reference.Branch;

    public string InstallationPath { get; }
    public IProductCatalog Manifest { get; }
    public VariableCollection ProductVariables { get; }
    public ProductInstallState InstallState { get; }

    public InstalledProduct(IProductReference reference, IProductCatalog manifest, string installationPath)
    {
        Requires.NotNull(reference, nameof(reference));
        Requires.NotNullOrEmpty(installationPath, nameof(installationPath));
        Requires.NotNull(manifest, nameof(manifest));
        _reference = reference;
        InstallationPath = installationPath;
        Manifest = manifest;
        ProductVariables = new VariableCollection();
    }

    public override string ToString()
    {
        return $"Product '{_reference.Name}, v:{_reference.Version?.ToString() ?? "NO_VERSION"}, Branch:{_reference.Branch?.ToString() ?? "NO_BRANCH"}' " +
               $"at {InstallationPath}";
    }
}