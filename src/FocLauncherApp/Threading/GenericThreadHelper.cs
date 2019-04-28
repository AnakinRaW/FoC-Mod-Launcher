using System;

namespace FocLauncherApp.Threading
{
    internal class GenericThreadHelper : ThreadHelper
    {
        protected override IDisposable GetInvocationWrapper()
        {
            return null;
        }
    }
}