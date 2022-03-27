using System.Threading;
using System.Threading.Tasks;

namespace Sklavenwalker.ProductUpdater
{
    public interface IUpdateCheckService
    {
        bool IsCheckingForUpdates { get; }

        Task<UpdateCheckResult> CheckForUpdates(UpdateRequest updateRequest, CancellationToken token = default);
    }
}