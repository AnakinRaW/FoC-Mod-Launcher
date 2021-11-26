using System;

namespace FocLauncher.Utilities
{
    // From https://github.com/microsoft/SEAL/blob/master/dotnet/src/tools/DisposableObject.cs (MIT License)
    // Keep Internal because otherwise this class gets shared across app-domains which tends to cause some problems.
    internal class DisposableObject : IDisposable
    {
        public bool IsDisposed { get; private set; }

        ~DisposableObject()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeManagedResources()
        {
        }

        protected virtual void DisposeNativeResources()
        {
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                    DisposeManagedResources();
                DisposeNativeResources();
                IsDisposed = true;
            }
        }
    }
}
