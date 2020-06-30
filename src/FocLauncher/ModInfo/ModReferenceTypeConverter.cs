using System;
using Newtonsoft.Json;

namespace FocLauncher.ModInfo
{
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
}