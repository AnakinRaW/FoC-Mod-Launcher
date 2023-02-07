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

    public VariableCollection ProductVariables { get; }

    public ProductInstallState InstallState { get; }

    public IProductManifest Manifest { get; }

    public InstalledProduct(IProductReference reference, string installationPath, IProductManifest manifest, VariableCollection? variables, ProductInstallState state = ProductInstallState.Installed)
    {
        Requires.NotNull(reference, nameof(reference));
        Requires.NotNull(manifest, nameof(manifest));
        Requires.NotNullOrEmpty(installationPath, nameof(installationPath));
        _reference = reference;
        InstallationPath = installationPath;
        ProductVariables = new VariableCollection();
        InstallState = state;
        Manifest = manifest;
        ProductVariables = variables ?? new VariableCollection();
    }

    public override string ToString()
    {
        return $"Product '{_reference.Name}, v:{_reference.Version?.ToString() ?? "NO_VERSION"}, Branch:{_reference.Branch?.ToString() ?? "NO_BRANCH"}' " +
               $"at {InstallationPath}";
    }
}