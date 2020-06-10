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
        ModVersion? Version { get; }
        IList<IModReference> Dependencies { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ModInfoData : IModIdentity
    {
        [JsonIgnore] private ModVersion? _modVersion;
        [JsonIgnore] private bool _versionDetermined;

        [JsonIgnore] public bool HasCustomObjects => Custom.Count > 0;

        [JsonIgnore] public bool HasSteamData => SteamData != null;

        [JsonIgnore] public bool HasDependencies => Dependencies.Count > 0;

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; internal set; }

        [JsonProperty("description")]
        public string Description { get; internal set; }

        [JsonProperty("icon")]
        public string Icon { get; internal set; }

        [JsonProperty("version")]
        private string StringVersion { get; set; }

        public ModVersion? Version
        {
            get
            {
                if (_modVersion is null && !_versionDetermined)
                {
                    _modVersion = string.IsNullOrEmpty(StringVersion) ? null : ModVersion.Parse(StringVersion);
                    _versionDetermined = true;
                }
                return _modVersion;
            }
            internal set => _modVersion = value;
        }
        
        [JsonProperty("custom")]
        public ICollection<JObject> Custom { get; internal set; }

        [JsonProperty("steamdata")]
        public SteamData? SteamData { get; internal set; }

        [JsonProperty("dependencies", ItemConverterType = typeof(ModReferenceTypeConverter))]
        public IList<IModReference> Dependencies { get; internal set; }
        
        internal ModInfoData()
        {
            Dependencies = new List<IModReference>();
            Custom = new List<JObject>();
        }

        internal ModInfoData(ModInfoData baseModInfoData) : this()
        {
            MergeFrom(baseModInfoData, true);
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Name))
                throw new ModInfoException("Name must not be null or empty.");
        }

        bool IEquatable<IModIdentity>.Equals(IModIdentity other)
        {
            throw new NotImplementedException();
        }

        internal void MergeFrom(ModInfoData target)
        {
            MergeFrom(target, false);
        }

        private void MergeFrom(ModInfoData target, bool fromConstructor)
        {
            Name = target.Name;

            if (fromConstructor || !string.IsNullOrEmpty(target.Description))
                Description = target.Description;

            if (fromConstructor || !string.IsNullOrEmpty(target.Icon))
                Icon = target.Icon;

            if (fromConstructor || target.Version != null)
                Version = target.Version;

            if (fromConstructor || target.HasCustomObjects)
            {
                foreach (var customObject in target.Custom)
                {
                    if (!fromConstructor && Custom.Contains(customObject)) 
                        continue;
                    Custom.Add(customObject);
                }
            }

            if (fromConstructor || target.HasSteamData) 
                SteamData = target.SteamData;

            if (fromConstructor || target.HasDependencies) 
                Dependencies = target.Dependencies;
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