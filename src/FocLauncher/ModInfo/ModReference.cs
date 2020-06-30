using System;
using FocLauncher.Mods;
using Newtonsoft.Json;

namespace FocLauncher.ModInfo
{
    public class ModReference : IModReference
    {
        /// <summary>
        /// Holds either the SteamId or the absolute location or the relative location to the game.
        /// </summary>
        [JsonProperty("identifier")]
        public string Identifier { get; private set; }

        [JsonProperty("modtype")]
        public ModType Type { get; private set; }

        bool IEquatable<IModReference>.Equals(IModReference other)
        {
            throw new NotImplementedException();
        }
    }
}