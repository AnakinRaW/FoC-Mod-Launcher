using System.Collections.Generic;

namespace FocLauncher.Versioning
{
    public interface IVersionComparer : IEqualityComparer<SemanticVersion>, IComparer<SemanticVersion>
    {
    }
}