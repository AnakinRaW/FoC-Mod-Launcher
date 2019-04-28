using System;

namespace FocLauncherApp.WaitDialog
{
    public class DisposableObject : IDisposable
    {
        private EventHandler _disposing;

        public event EventHandler Disposing
        {
            add
            {
                ThrowIfDisposed();
                _disposing = (EventHandler) Delegate.Combine(_disposing, value);
            }
            remove => _disposing = (EventHandler) Delegate.Remove(_disposing, value);
        }

        public bool IsDisposed { get; private set; }

        ~DisposableObject()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            try
            {
                if (disposing)
                {
                    _disposing?.Invoke(this, EventArgs.Empty);
                    _disposing = null;
                    DisposeManagedResources();
                }

                DisposeNativeResources();
            }
            finally
            {
                IsDisposed = true;
            }
        }

        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        protected virtual void DisposeManagedResources()
        {
        }

        protected virtual void DisposeNativeResources()
        {
        }
    }
}