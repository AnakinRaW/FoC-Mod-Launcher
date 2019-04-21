using System.Threading;

namespace FocLauncher.Utilities
{
    public abstract class ReusableResourceStoreBase<TResource> where TResource : class
    {
        private TResource _resource;

        protected TResource AcquireCore()
        {
            return Interlocked.Exchange(ref _resource, default);
        }

        internal void ReleaseCore(TResource value)
        {
            if (!Cleanup(value))
                return;
            Interlocked.Exchange(ref _resource, value);
        }

        protected virtual bool Cleanup(TResource value)
        {
            return true;
        }
    }
}