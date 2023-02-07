using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.ProductMetadata.Catalog;
using Semver;

namespace AnakinRaW.ProductMetadata.Services;

public interface IBranchManager
{
    Task<IEnumerable<ProductBranch>> GetAvailableBranches();

    ProductBranch GetBranchFromVersion(SemVersion version);

    Task<IProductManifest> GetManifest(IProductReference branch, CancellationToken token = default);
}