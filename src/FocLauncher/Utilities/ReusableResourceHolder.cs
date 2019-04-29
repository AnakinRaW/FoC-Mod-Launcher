using System;

namespace FocLauncher.Core.Utilities
{
    public struct ReusableResourceHolder<TResource> : IDisposable where TResource : class
    {
        private readonly ReusableResourceStoreBase<TResource> _store;

        public TResource Resource { get; private set; }

        internal ReusableResourceHolder(ReusableResourceStoreBase<TResource> store, TResource value)
        {
            _store = store;
            Resource = value;
        }

        public void Dispose()
        {
            _store.ReleaseCore(Resource);
            Resource = default;
        }
    }
}