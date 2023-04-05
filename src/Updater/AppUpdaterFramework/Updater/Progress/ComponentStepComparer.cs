using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpdaterFramework.Updater.Progress;

internal class ComponentStepComparer : IEqualityComparer<IComponentStep>
{
    internal static readonly IEqualityComparer<IComponentStep> Default = new ComponentStepComparer();
    private readonly IEqualityComparer<IProductComponentIdentity> _comparer;

    internal ComponentStepComparer(IEqualityComparer<IProductComponentIdentity>? comparer = null)
    {
        _comparer = comparer ?? ProductComponentIdentityComparer.Default;
    }

    public bool Equals(IComponentStep x, IComponentStep y)
    {
        return _comparer.Equals(x.Component, y.Component);
    }

    public int GetHashCode(IComponentStep obj)
    {
        return _comparer.GetHashCode(obj.Component);
    }
}