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
    public interface IModIdentity : IEquatable<IModIdentity>
    {
        string Name { get; }
        ModVersion Version { get; }
        IList<IModReference> Dependencies { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ModInfoData : IModIdentity
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("icon")]
        public string Icon { get; private set; }

        [JsonProperty("version")]
        private string StringVersion { get; set; }

        // TODO: Make nullable
        public ModVersion Version => ModVersion.Parse(StringVersion);

        [JsonProperty("custom")]
        public JObject Custom { get; set; }

        [JsonProperty("steamdata")]
        public SteamData SteamData { get; private set; }

        [JsonProperty("dependencies", ItemConverterType = typeof(ModReferenceTypeConverter))]
        public IList<IModReference> Dependencies { get; private set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Name))
                throw new ModInfoException("Name must not be null or empty.");
        }

        internal ModInfoData()
        {
            Dependencies = new List<IModReference>();
        }

        bool IEquatable<IModIdentity>.Equals(IModIdentity other)
        {
            throw new NotImplementedException();
        }
    }

    internal class ModReferenceTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<ModReference>(reader);
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }

    public interface IModReference : IEquatable<IModReference>
    {
        string Identifier { get; }

        ModType Type { get; }
    }

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

    internal static class ModInfoExtension
    {
        public static string GetAbsolutePath(this IModReference modReference, IGame game)
        {
            if (modReference is null)
                throw new ArgumentNullException(nameof(modReference));
            if (string.IsNullOrEmpty(modReference.Identifier))
                throw new InvalidOperationException("location must not be null or empty");
            if (modReference.Type == ModType.Virtual || modReference.Type == ModType.Workshops)
                throw new InvalidOperationException("ModReference must be a ModType of 'Default'");
            if (Path.IsPathRooted(modReference.Identifier))
                return modReference.Identifier;
            if (game is null)
                throw new ArgumentNullException(nameof(game));
            return Path.Combine(game.Directory.FullName, modReference.Identifier);
        }
    }
}