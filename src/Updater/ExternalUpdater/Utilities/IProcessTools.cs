using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace AnakinRaW.ExternalUpdater.Utilities;

internal interface IProcessTools
{
    void StartApplication(IFileInfo application, ExternalUpdaterResult appStartOptions, bool elevate = false);

    Task<bool> WaitForExitAsync(int? pid, CancellationToken token);
}