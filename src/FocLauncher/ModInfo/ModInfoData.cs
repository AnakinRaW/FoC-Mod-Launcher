using System;
using System.Collections.Generic;
using System.IO;
using FocLauncher.Game;
using FocLauncher.Mods;
using FocLauncher.Versioning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FocLauncher.ModInfo
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ModInfoData
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("icon")]
        public string Icon { get; private set; }

        [JsonProperty("version")]
        private string StringVersion { get; set; }

        public ModVersion Version => ModVersion.Parse(StringVersion);

        [JsonProperty("custom")]
        public JObject Custom { get; set; }

        [JsonProperty("steamdata")]
        public SteamData SteamData { get; private set; }

        [JsonProperty("dependencies")]
        public IList<ModReference> Dependencies { get; private set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Name))
                throw new ModInfoException("Name must not be null or empty.");
        }

        public ModInfoData()
        {
            Dependencies = new List<ModReference>();
        }
    }

    public class ModReference
    {
        /// <summary>
        /// Holds either the SteamId or the absolute location or the relative location to the game.
        /// </summary>
        [JsonProperty("location")]
        public string Location { get; private set; }

        [JsonProperty("modtype")]
        public ModType ModType { get; private set; }
    }

    internal static class ModInfoExtension
    {
        public static string GetAbsolutePath(this ModReference modReference, IGame game)
        {
            if (modReference is null)
                throw new ArgumentNullException(nameof(modReference));
            if (string.IsNullOrEmpty(modReference.Location))
                throw new InvalidOperationException("location must not be null or empty");
            if (modReference.ModType == ModType.Virtual || modReference.ModType == ModType.Workshops)
                throw new InvalidOperationException("ModReference must be a ModType of 'Default'");
            if (Path.IsPathRooted(modReference.Location))
                return modReference.Location;
            if (game is null)
                throw new ArgumentNullException(nameof(game));
            return Path.Combine(game.Directory.FullName, modReference.Location);
        }
    }
}