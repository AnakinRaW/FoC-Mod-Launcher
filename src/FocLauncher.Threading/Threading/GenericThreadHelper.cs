using System;

namespace FocLauncher.Threading
{
    internal class GenericThreadHelper : ThreadHelper
    {
        protected override IDisposable GetInvocationWrapper()
        {
            return null;
        }
    }
}