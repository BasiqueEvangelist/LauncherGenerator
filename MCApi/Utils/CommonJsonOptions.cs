using System.Text.Json;
using System.Text.Json.Serialization;

namespace MCApi.Utils;

public static class CommonJsonOptions
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    static CommonJsonOptions()
    {
        Options.Converters.Add(new DateTimeConverter());
        Options.Converters.Add(new DateTimeOffsetConverter());
    }

    private class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString() ?? throw new JsonException());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value:O}");
        }
    }

    private class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                return DateTimeOffset.Parse(reader.GetString() ?? throw new JsonException());
            }
            catch (Exception ex)
            { 
                throw; 
            }
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value:O}");
        }
    }
}