using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpdaterFramework.Updater.Progress;

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