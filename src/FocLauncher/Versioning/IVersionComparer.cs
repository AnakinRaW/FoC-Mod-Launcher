using System.Collections.Generic;

namespace FocLauncher.Core.Versioning
{
    public interface IVersionComparer : IEqualityComparer<SemanticVersion>, IComparer<SemanticVersion>
    {
    }
}