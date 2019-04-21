using System.Text;

namespace FocLauncher.Utilities
{
    public sealed class ReusableStringBuilder : ReusableResourceStore<StringBuilder, int>
    {
        private static readonly ReusableStringBuilder DefaultInstance = new ReusableStringBuilder();
        private readonly int _maximumCacheCapacity;

        public ReusableStringBuilder(int maximumCacheCapacity = 512)
        {
            _maximumCacheCapacity = maximumCacheCapacity;
        }

        public static ReusableResourceHolder<StringBuilder> AcquireDefault(
            int capacity)
        {
            return DefaultInstance.Acquire(capacity);
        }

        protected override StringBuilder Allocate(int constructorParameter)
        {
            return new StringBuilder(constructorParameter);
        }

        protected override bool Cleanup(StringBuilder value)
        {
            if (value.Capacity > _maximumCacheCapacity)
                return false;
            value.Clear();
            return true;
        }

        protected override bool CanReuse(StringBuilder value, int parameter)
        {
            return value.Capacity >= parameter;
        }
    }
}