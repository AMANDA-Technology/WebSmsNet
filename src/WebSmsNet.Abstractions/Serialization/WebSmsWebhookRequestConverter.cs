using System.Text.Json;
using System.Text.Json.Serialization;
using WebSmsNet.Abstractions.Models;

namespace WebSmsNet.Abstractions.Serialization;

/// <summary>
/// Responsible for converting <see cref="WebSmsWebhookRequest.Base"/> objects to and from JSON
/// during serialization and deserialization. The discriminator is the <c>messageType</c>
/// JSON property and routes to <see cref="WebSmsWebhookRequest.Text"/>,
/// <see cref="WebSmsWebhookRequest.Binary"/>, or <see cref="WebSmsWebhookRequest.DeliveryReport"/>.
/// </summary>
public class WebSmsWebhookRequestConverter : JsonConverter<WebSmsWebhookRequest.Base>
{
    /// <inheritdoc />
    public override WebSmsWebhookRequest.Base? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (root.ValueKind is not JsonValueKind.Object)
            throw new JsonException($"Expected JSON object for webhook request but got {root.ValueKind}.");

        if (!root.TryGetProperty("messageType", out var messageTypeElement))
            throw new JsonException("Missing messageType property.");

        if (messageTypeElement.ValueKind is not JsonValueKind.String)
            throw new JsonException($"Property 'messageType' must be a string but was {messageTypeElement.ValueKind}.");

        var messageType = messageTypeElement.GetString();

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
        // Serialize the object based on its runtime type so the discriminator and subtype-only
        // properties are emitted correctly.
        var type = value.GetType();
        JsonSerializer.Serialize(writer, value, type, options);
    }
}
