using Vanara.PInvoke.NetListMgr;

namespace AnakinRaW.AppUpaterFramework.Services;

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