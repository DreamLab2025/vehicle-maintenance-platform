using System.Text.Json;
using System.Text.Json.Serialization;

namespace Verendar.Common.Shared;

public sealed class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private static readonly string[] Formats = ["HH:mm:ss.fffffff", "HH:mm:ss", "HH:mm"];

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null)
            throw new JsonException("Expected a time string but got null.");

        if (TimeOnly.TryParseExact(value, Formats, null, System.Globalization.DateTimeStyles.None, out var result))
            return result;

        throw new JsonException($"The JSON value '{value}' could not be converted to TimeOnly. Expected formats: HH:mm, HH:mm:ss, or HH:mm:ss.fffffff.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("HH:mm:ss"));
    }
}
