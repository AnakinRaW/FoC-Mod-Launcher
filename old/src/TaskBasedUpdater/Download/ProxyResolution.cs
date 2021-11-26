namespace TaskBasedUpdater.Download
{
    internal enum ProxyResolution
    {
        Default,
        DefaultCredentialsOrNoAutoProxy,
        NetworkCredentials,
        DirectAccess,
        Error,
    }
}