using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.Wpf.CommandBar.Input.Focus;

namespace Sklavenwalker.Wpf.CommandBar
{
    public static class ExtendedCommandBarLibrary
    {
        public static IServiceCollection InitializeLibrary(IServiceCollection sc)
        {
            CommandFocusManager.Initialize();
            return sc;
        }
    }
}
