using System;

namespace FocLauncher.Theming
{
    public interface ITheme : IEquatable<ITheme>
    {
        string Name { get; }

        Uri GetResourceUri();
    }
}
