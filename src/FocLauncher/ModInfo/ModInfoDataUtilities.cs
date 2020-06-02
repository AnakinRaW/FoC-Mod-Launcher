using System;
using System.Diagnostics;

namespace FocLauncher.ModInfo
{
    public static class ModInfoDataUtilities
    {
        public static ModInfoData Merge(this ModInfoData target, ModInfoData? baseModInfo)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            if (baseModInfo is null)
                return target;
            // TODO:
            Debugger.Break();
            return null;
        }
    }
}
