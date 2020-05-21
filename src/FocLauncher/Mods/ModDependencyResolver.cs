using System.Collections.Generic;

namespace FocLauncher.Mods
{
    public class ModDependencyResolver // : DisposableObject
    {
        private readonly IMod _mod;

        public ModDependencyResolver(IMod mod)
        {
            _mod = mod;
        }

        public IList<IMod> Resolve(bool fullRecursive)
        {
            return null;
        }
    }
}