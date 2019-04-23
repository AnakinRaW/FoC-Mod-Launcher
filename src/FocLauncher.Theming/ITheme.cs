using System;

namespace FocLauncher.Theming
{
    public interface ITheme
    {
        string Name { get; }

        Uri GetResourceUri();
    }
}
