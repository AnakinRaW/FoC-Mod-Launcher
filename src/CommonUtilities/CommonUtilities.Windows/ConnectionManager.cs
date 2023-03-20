using Vanara.PInvoke.NetListMgr;

namespace AnakinRaW.CommonUtilities.Windows;

public class ConnectionManager : IConnectionManager
{
    public static readonly IConnectionManager Instance = new ConnectionManager();

    private readonly INetworkListManager _networkListManager;

    private ConnectionManager()
    {
        _networkListManager = (INetworkListManager)new NetworkListManager();
    }
    
    public bool HasInternetConnection()
    {
        return _networkListManager.IsConnectedToInternet;
    }
}