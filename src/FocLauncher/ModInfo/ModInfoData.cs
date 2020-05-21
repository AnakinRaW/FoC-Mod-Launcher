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

        public void Validate()
        {
            if (string.IsNullOrEmpty(Name))
                throw new ModInfoException("Name must not be null or empty.");
        }
    }
}