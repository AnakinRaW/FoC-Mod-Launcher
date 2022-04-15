using Validation;

namespace Sklavenwalker.ProductMetadata.Component;

/// <summary>
/// A component representing a single file.
/// </summary>
/// <remarks>
/// The property <see cref="IInstallableComponent.DetectConditions"/> shall not get used for installation detection.
/// </remarks>
public class SingleFileComponent : InstallableComponent
{
    public override ComponentType Type => ComponentType.File;

    /// <summary>
    /// Relative path without file name where the file shall get installed to. May contain environment variables.
    /// </summary>
    public string InstallPath { get; }

    /// <summary>
    /// Actual path of the file.
    /// </summary>
    public string? FilePath { get; }

    public SingleFileComponent(IProductComponentIdentity identity, string installPath, string? filePath) : base(identity)
    {
        Requires.NotNullOrEmpty(installPath, nameof(installPath));
        InstallPath = installPath;
        FilePath = filePath;
    }
}