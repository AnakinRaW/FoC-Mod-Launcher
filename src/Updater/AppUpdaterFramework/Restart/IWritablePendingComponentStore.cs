using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Restart;

internal interface IWritablePendingComponentStore : IPendingComponentStore
{
    void AddComponent(IInstallableComponent component);

    void Clear();
}