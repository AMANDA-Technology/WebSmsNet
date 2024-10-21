using System.Text.Json.Serialization;
using WebSmsNet.Abstractions.Models.Enums;

namespace WebSmsNet.Abstractions.Models;

/// <summary>
/// Represents the response after processing a webhook request.
/// </summary>
public class WebSmsWebhookResponse
{
    /// <summary>
    /// Status code after processed request
    /// </summary>
    [JsonPropertyName("statusCode")]
    [JsonConverter(typeof(JsonNumberEnumConverter<WebSmsStatusCode>))]
    public required WebSmsStatusCode StatusCode { get; init; }

    /// <summary>
    /// Description of the response status
    /// </summary>
    [JsonPropertyName("statusMessage")]
    public required string StatusMessage { get; init; }
}
