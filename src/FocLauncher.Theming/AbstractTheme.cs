using System;

namespace FocLauncher.Theming
{
    public abstract class AbstractTheme : ITheme
    {
        public abstract string Name { get; }

        public abstract Uri GetResourceUri();

        public bool Equals(ITheme other)
        {
            if (other == null)
                return false;
            if (GetResourceUri() == other.GetResourceUri() && Name == other.Name)
                return true;
            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals(obj as ITheme);
        }

        public override int GetHashCode()
        {
            var hasCode = 352033288;
            hasCode = hasCode * -152113495 + Name.GetHashCode();
            hasCode = hasCode * -152113495 + GetResourceUri().GetHashCode();
            return hasCode;
        }
    }
}