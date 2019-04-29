using System.IO;
using FocLauncher.Core.Versioning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FocLauncher.Core.ModInfo
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct ModInfoFile
    {
        [JsonProperty("name")]
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

        public static ModInfoFile Parse(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException();
            if (Path.GetExtension(filePath) != ".json")
                throw new ModInfoFileException("File must be a json");
            var text = File.ReadAllText(filePath);
            var modInfo = JsonConvert.DeserializeObject<ModInfoFile>(text);
            if (string.IsNullOrWhiteSpace(modInfo.Name))
                throw new ModInfoFileException("No mod name was specified");
            return modInfo;
        }

        public static bool TryParse(string filePath, out ModInfoFile modInfo)
        {
            modInfo = default;
            try
            {
                modInfo = Parse(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
