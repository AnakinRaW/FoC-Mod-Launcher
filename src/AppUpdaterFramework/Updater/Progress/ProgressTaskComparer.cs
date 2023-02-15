using System.Collections.Generic;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpaterFramework.Updater.Progress;

internal class ProgressTaskComparer : IEqualityComparer<IProgressTask>
{
    internal static readonly IEqualityComparer<IProgressTask> Default = new ProgressTaskComparer();
    private readonly IEqualityComparer<IProductComponentIdentity> _comparer;

    internal ProgressTaskComparer(IEqualityComparer<IProductComponentIdentity>? comparer = null)
    {
        _comparer = comparer ?? ProductComponentIdentityComparer.Default;
    }

    public bool Equals(IProgressTask x, IProgressTask y)
    {
        return _comparer.Equals(x.Component, y.Component);
    }

    public int GetHashCode(IProgressTask obj)
    {
        return _comparer.GetHashCode(obj.Component);
    }
}