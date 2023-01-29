using Vanara.PInvoke.NetListMgr;

namespace AnakinRaW.ProductUpdater.Services;

internal class ConnectionManager : IConnectionManager
{
    private readonly INetworkListManager _networkListManager;

    public ConnectionManager()
    {
        _networkListManager = (INetworkListManager) new NetworkListManager();
    }
    
    public bool HasInternetConnection()
    {
        return _networkListManager.IsConnectedToInternet;
    }
}

public interface IConnectionManager
{
    bool HasInternetConnection();
}