using System;

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
            target.Validate();
            baseModInfo.Validate();
            var newModInfo = new ModInfoData(baseModInfo);
            newModInfo.MergeFrom(target);
            return newModInfo;
        }
    }
}
