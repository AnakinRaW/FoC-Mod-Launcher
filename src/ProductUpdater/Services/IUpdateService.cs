using System.Threading;
using System.Threading.Tasks;
using Sklavenwalker.ProductUpdater.Catalog;

namespace Sklavenwalker.ProductUpdater.Services;

public interface IUpdateService
{
    Task<UpdateCheckResult> CheckForUpdates(UpdateRequest request, CancellationToken token = default);

    Task<bool> Update(IUpdateCatalog updateCatalog, CancellationToken token = default);
}