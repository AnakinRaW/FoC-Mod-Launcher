namespace AnakinRaW.AppUpaterFramework.Updater;

internal class UpdateSession : IUpdateSession
{
    public void Cancel()
    {
        
    }
}

public interface IUpdateSession
{
    void Cancel();
}