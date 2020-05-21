using System;
using System.Collections.Generic;
using FocLauncher.Game;
using FocLauncher.ModInfo;

namespace FocLauncher.Mods
{
    public sealed class VirtualMod : ModBase
    {
        public VirtualMod(IGame game, ModInfoData modInfoData) : base(game, ModType.Virtual, modInfoData)
        {
            // TODO: modinfo dependencies must not be null or empty!
        }

        //public VirtualMod(IGame game, ModInfoFile modInfoFile) : base(game, ModType.Virtual)
        //{
        //}
        
        public VirtualMod(string name, IGame game, IList<IMod> dependencies) : base(name, game, ModType.Virtual)
        {
            // TODO: dependencies must not be null or empty!
        }

        public override string ToArgs(bool includeDependencies)
        {
            if (Virtual && !includeDependencies)
                throw new InvalidOperationException("A virtual mod cannot yield arguments. Use parameter 'includeDependencies' instead!");

            return string.Empty;
        }

        protected override bool ResolveDependenciesCore()
        {
            throw new NotImplementedException();
        }
    }
}