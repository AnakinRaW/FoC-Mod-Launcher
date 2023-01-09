using Semver;

namespace AnakinRaW.ProductMetadata;

public interface IProductReference
{
    string Name { get; }

    SemVersion? Version { get; }
        
    ProductBranch? Branch { get; }
}