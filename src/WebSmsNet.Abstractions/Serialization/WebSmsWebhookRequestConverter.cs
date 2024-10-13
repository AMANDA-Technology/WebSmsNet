using System.Text.Json;
using System.Text.Json.Serialization;
using WebSmsNet.Abstractions.Models;

namespace WebSmsNet.Abstractions.Serialization;

/// <summary>
/// Responsible for converting WebSmsWebhookRequest objects to and from JSON
/// during serialization and deserialization.
/// </summary>
public class WebSmsWebhookRequestConverter : JsonConverter<WebSmsWebhookRequest.Base>
{
    /// <inheritdoc />
    public override WebSmsWebhookRequest.Base? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        // Check if the messageType property exists
        if (!root.TryGetProperty("messageType", out var messageTypeElement))
            throw new JsonException("Missing messageType property.");

        // Get the value of messageType
        var messageType = messageTypeElement.GetString();

        // Determine the type based on messageType
        return messageType switch
        {
            "text" => JsonSerializer.Deserialize<WebSmsWebhookRequest.Text>(root.GetRawText(), options),
            "binary" => JsonSerializer.Deserialize<WebSmsWebhookRequest.Binary>(root.GetRawText(), options),
            "deliveryReport" => JsonSerializer.Deserialize<WebSmsWebhookRequest.DeliveryReport>(root.GetRawText(), options),
            _ => throw new JsonException($"Unknown messageType: {messageType}")
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, WebSmsWebhookRequest.Base value, JsonSerializerOptions options)
    {
        // Serialize the object based on its runtime type
        var type = value.GetType();
        JsonSerializer.Serialize(writer, value, type, options);
    }
}
