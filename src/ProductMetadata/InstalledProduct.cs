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

    public InstalledProduct(IProductReference reference, string installationPath, ProductInstallState state = ProductInstallState.Installed)
    {
        Requires.NotNull(reference, nameof(reference));
        Requires.NotNullOrEmpty(installationPath, nameof(installationPath));
        _reference = reference;
        InstallationPath = installationPath;
        ProductVariables = new VariableCollection();
        InstallState = state;
    }

    public override string ToString()
    {
        return $"Product '{_reference.Name}, v:{_reference.Version?.ToString() ?? "NO_VERSION"}, Branch:{_reference.Branch?.ToString() ?? "NO_BRANCH"}' " +
               $"at {InstallationPath}";
    }
}