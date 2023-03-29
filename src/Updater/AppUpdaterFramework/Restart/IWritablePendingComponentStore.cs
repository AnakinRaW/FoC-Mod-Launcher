namespace AnakinRaW.AppUpdaterFramework.Restart;

internal interface IWritablePendingComponentStore : IPendingComponentStore
{
    void AddComponent(PendingComponent component);

    void Clear();
}