using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Restart;

internal interface IWritableDeferredComponentStore : IDeferredComponentStore
{
    void AddComponent(IInstallableComponent component);

    void Clear();
}