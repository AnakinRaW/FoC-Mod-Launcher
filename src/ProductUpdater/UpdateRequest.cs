using AnakinRaW.ProductMetadata;

namespace AnakinRaW.ProductUpdater;

public class UpdateRequest
{
    public InstalledProduct Product { get; }

    public string? Branch { get; }

    public UpdateRequest(InstalledProduct product, string? branch = null)
    {
        Product = product;
        Branch = branch;
    }
}