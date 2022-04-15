using System.Threading;
using System.Threading.Tasks;
using Sklavenwalker.ProductMetadata.Catalog;

namespace Sklavenwalker.ProductUpdater;

public interface IUpdateCheckService
{
    bool IsCheckingForUpdates { get; }

    Task<UpdateCheckResult> CheckForUpdates(CatalogLocation updateRequest, CancellationToken token = default);
}