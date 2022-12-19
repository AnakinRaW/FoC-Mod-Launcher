using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Semver;
using Sklavenwalker.ProductMetadata.Catalog;

namespace Sklavenwalker.ProductMetadata.Services;

public interface IBranchManager
{
    Task<IEnumerable<ProductBranch>> GetAvailableBranches();

    ProductBranch GetBranchFromVersion(SemVersion version);

    Task<IProductCatalog> GetManifest(IProductReference branch, CancellationToken token = default);
}