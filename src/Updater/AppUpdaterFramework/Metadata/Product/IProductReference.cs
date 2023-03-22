using Semver;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Product;

public interface IProductReference
{
    string Name { get; }

    SemVersion? Version { get; }
        
    ProductBranch? Branch { get; }
}