using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Service_API.Helpers
{
    public class NullableIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return null;
                }
                if (int.TryParse(stringValue, out int intVal))
                {
                    return intVal;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            throw new JsonException("Cannot convert value to Nullable Int.");
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    public class NullableDecimalConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return null;
                }
                if (decimal.TryParse(stringValue, out decimal decimalVal))
                {
                    return decimalVal;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            throw new JsonException("Cannot convert value to Nullable Decimal.");
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    public class NullableLongConverter : JsonConverter<long?>
    {
        public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return null;
                }
                if (long.TryParse(stringValue, out long longVal))
                {
                    return longVal;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64();
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            throw new JsonException("Cannot convert value to Nullable Long.");
        }

        public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
