using Validation;

namespace Sklavenwalker.ProductMetadata.Component;

public class FileItem
{
    public string FilePath { get; }

    public ComponentIntegrityInformation IntegrityInformation { get; }

    public FileItem(string filePath, ComponentIntegrityInformation integrityInformation)
    {
        Requires.NotNullOrEmpty(filePath, nameof(filePath));
        FilePath = filePath;
        IntegrityInformation = integrityInformation;
    }
}