using System;
using Newtonsoft.Json;

namespace chargebook.data.infrastructure {
    public class TimeZoneInfoJsonConverter : JsonConverter {

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteValue(value is TimeZoneInfo timeZone ? timeZone.Id : "null");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var valueString = (string) reader.Value;
            if (string.IsNullOrEmpty(valueString) || valueString == "null") {
                return null;
            }
            return TimeZoneInfo.FindSystemTimeZoneById(valueString);
        }

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(TimeZoneInfo);
        }
    }
}