using System.Text.Json;

namespace Verendar.AppHost.Tests.Support;

internal static class ApiResponseJsonReader
{
    public static string? ReadFirstArrayItemStringProperty(string apiResponseJson, string propertyName)
    {
        using var document = JsonDocument.Parse(apiResponseJson);

        if (!document.RootElement.TryGetProperty("data", out var data) ||
            data.ValueKind != JsonValueKind.Array ||
            data.GetArrayLength() == 0)
        {
            return null;
        }

        var firstItem = data[0];
        if (!firstItem.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.GetString();
    }
}
