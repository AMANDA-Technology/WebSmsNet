using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using WebSmsNet.Abstractions.Models.Enums;

namespace WebSmsNet.Abstractions.Models;

/// <summary>
/// Represents a request to send a text SMS via the <c>/rest/smsmessaging/text</c> endpoint.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class TextSmsSendRequest : SmsSendRequest
{
    /// <summary>
    /// Optional. Maximum number of SMS segments to be generated for this message.
    /// If the system would generate more than this number of SMS segments, the API returns
    /// status code <see cref="WebSmsStatusCode.MaxSmsPerMessageExceeded"/> (4026).
    /// When omitted (or set to <c>0</c>), no limitation is applied.
    /// </summary>
    [JsonPropertyName("maxSmsPerMessage")]
    public int? MaxSmsPerMessage { get; set; }

    /// <summary>
    /// Required. The content of the SMS message (UTF-8 encoded).
    /// </summary>
    [JsonPropertyName("messageContent")]
    public required string MessageContent { get; set; }

    /// <summary>
    /// Optional. Type of message
    /// (<see cref="Enums.MessageType.Default"/> for a regular SMS, or <see cref="Enums.MessageType.Voice"/> for a voice message).
    /// </summary>
    [JsonPropertyName("messageType")]
    public MessageType? MessageType { get; set; }
}
