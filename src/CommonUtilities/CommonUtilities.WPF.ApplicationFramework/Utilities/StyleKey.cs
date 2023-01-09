using System.Reflection;
using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Utilities;

internal class StyleKey<T> : ResourceKey
{
    private Assembly? _assembly;

    public override Assembly Assembly
    {
        get
        {
            var assembly = _assembly;
            if (assembly != null)
                return assembly;
            return _assembly = typeof(T).Assembly;
        }
    }
}