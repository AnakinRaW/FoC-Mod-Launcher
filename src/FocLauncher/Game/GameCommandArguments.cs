using System.Collections.Generic;
using FocLauncher.Mods;

namespace FocLauncher.Game
{
    public sealed class GameCommandArguments
    {
        public bool IgnoreAsserts { get; set; }

        public bool NoArtProcess { get; set; }

        public bool Windowed { get; set; }

        /// <summary>
        /// An ordered collection of Mod that shall get executed.
        /// The first mod in the list resembles the base mod, while the last mod in the list resembles the last sub mod.
        /// <remarks>Note that the order of this list actually is reverse compared to the CLI arguments, where the first item is the sub mod. </remarks>
        /// </summary>
        public IList<IMod>? Mods { get; set; }
    }
}