using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foundation
{
    public class JSON
    {
        private const string DATE_FORMAT_FOR_MICROSECOND_PRECISION_WITH_UTC_ZONE_INDICATOR = "yyyy-MM-ddTHH:mm:ss.ffffffZ";

        /// <summary>
        /// 
        /// This allows an enum value to be serialized or deserialized as a string
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class EnumToStringConverter<T> : JsonConverter<T> where T : Enum
        {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    string value = reader.GetString();
                    return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
                }

                throw new JsonException($"Unable to convert \"{reader.GetString()}\" to Enum.");
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        public class UtcDateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string? dateString = reader.GetString();

                if (string.IsNullOrEmpty(dateString))
                {
                    throw new JsonException("DateTime string cannot be null or empty.");
                }

                // Parse with RoundtripKind to preserve UTC if 'Z' is present
                if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
                {
                    // Ensure the result is in UTC
                    return result.Kind == DateTimeKind.Utc ? result : result.ToUniversalTime();
                }

                throw new JsonException($"Failed to parse '{dateString}' as a valid DateTime.");
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToUniversalTime().ToString(DATE_FORMAT_FOR_MICROSECOND_PRECISION_WITH_UTC_ZONE_INDICATOR, CultureInfo.InvariantCulture));
            }
        }
    }
}
