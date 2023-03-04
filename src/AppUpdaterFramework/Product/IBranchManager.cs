using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using Semver;

namespace AnakinRaW.AppUpdaterFramework.Product;

public interface IBranchManager
{
    Task<IEnumerable<ProductBranch>> GetAvailableBranches();

    ProductBranch GetBranchFromVersion(SemVersion version);

    Task<IProductManifest> GetManifest(IProductReference branch, CancellationToken token = default);
}