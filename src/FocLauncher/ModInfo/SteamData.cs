using System.Collections.Generic;
using Newtonsoft.Json;

namespace FocLauncher.ModInfo
{
    public struct SteamData
    {
        [JsonProperty("publishedfileid")]
        public string ID { get; private set; }

        [JsonProperty("contentfolder")]
        public string ContentFolder { get; private set; }

        [JsonProperty("previewfile")]
        public string PreviewFile { get; private set; }

        [JsonProperty("visibility")]
        public SteamWorkshopVisibility Visibility { get; private set; }

        [JsonProperty("title")]
        public string Title { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("metadata")]
        public string Metadata { get; private set; }

        [JsonProperty("tags")]
        public IEnumerable<string> Tags { get; private set; }
    }
}