using AnakinRaW.AppUpaterFramework.Services;
using Vanara.PInvoke.NetListMgr;

namespace FocLauncher.Services;

internal class ConnectionManager : IConnectionManager
{
    private readonly INetworkListManager _networkListManager;

    public ConnectionManager()
    {
        _networkListManager = (INetworkListManager)new NetworkListManager();
    }
    
    public bool HasInternetConnection()
    {
        return _networkListManager.IsConnectedToInternet;
    }
}