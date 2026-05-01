using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using WebSmsNet.Abstractions.Models.Enums;

namespace WebSmsNet.Abstractions.Models;

/// <summary>
/// Represents the response after attempting to send a message via the WebSms API.
/// </summary>
[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed record MessageSendResponse
{
    /// <summary>
    /// The identifier for the message as specified by the client in the request, echoed back by the API.
    /// <c>null</c> if no <see cref="SmsSendRequest.ClientMessageId"/> was provided in the request.
    /// </summary>
    [JsonPropertyName("clientMessageId")]
    public string? ClientMessageId { get; init; }

    /// <summary>
    /// The number of SMS parts the message was divided into.
    /// </summary>
    [JsonPropertyName("smsCount")]
    public required int SmsCount { get; init; }

    /// <summary>
    /// The status code returned by the API. See <see cref="WebSmsStatusCode"/> for the documented values.
    /// </summary>
    [JsonPropertyName("statusCode")]
    [JsonConverter(typeof(JsonNumberEnumConverter<WebSmsStatusCode>))]
    public required WebSmsStatusCode StatusCode { get; init; }

    /// <summary>
    /// A message describing the status of the API response.
    /// </summary>
    [JsonPropertyName("statusMessage")]
    public required string StatusMessage { get; init; }

    /// <summary>
    /// An identifier for the message transfer assigned by the API.
    /// </summary>
    [JsonPropertyName("transferId")]
    public required string TransferId { get; init; }
}
