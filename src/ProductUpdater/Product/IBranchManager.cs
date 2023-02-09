using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using Semver;

namespace AnakinRaW.AppUpaterFramework.Product;

public interface IBranchManager
{
    Task<IEnumerable<ProductBranch>> GetAvailableBranches();

    ProductBranch GetBranchFromVersion(SemVersion version);

    Task<IProductManifest> GetManifest(IProductReference branch, CancellationToken token = default);
}