using Semver;

namespace AnakinRaW.AppUpaterFramework.Metadata.Product;

public interface IProductReference
{
    string Name { get; }

    SemVersion? Version { get; }
        
    ProductBranch? Branch { get; }
}