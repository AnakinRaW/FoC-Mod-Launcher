using System.Linq;
using Validation;

namespace Sklavenwalker.ProductUpdater
{
    internal static class Extensions
    {
        public static bool RequiresUpdate(this IUpdateCatalog updateCatalog)
        {
            Requires.NotNull(updateCatalog, nameof(updateCatalog));
            if (!updateCatalog.Items.Any())
                return false;
            return updateCatalog.Items.Count() != updateCatalog.ComponentsToKeep.Count();
        }
    }
}