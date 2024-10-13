using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebSmsNet.Abstractions.Serialization;

/// <summary>
/// Provides JSON serialization options for WebSmsNet.
/// </summary>
public static class WebSmsJsonSerialization
{
    /// <summary>
    /// Gets the default JSON serializer options configured for WebSmsNet.
    /// These options include the following configurations:
    /// - Using JSON serializer defaults for web applications.
    /// - Ignoring null values during writing.
    /// - Disabling indentation for compact output.
    /// - Converting enums to camel case strings.
    /// </summary>
    public static JsonSerializerOptions DefaultOptions => new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new WebSmsWebhookRequestConverter()
        }
    };
}
