using System.Reflection;
using System.Windows;

namespace Sklavenwalker.Wpf.CommandBar.Theming;

internal sealed class StyleKey<T> : ResourceKey
{
    private Assembly? _assembly;

    public override Assembly Assembly
    {
        get
        {
            var assembly = _assembly;
            return assembly != null ? assembly : _assembly = typeof(T).Assembly;
        }
    }
}