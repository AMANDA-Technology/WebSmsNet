using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using WebSmsNet.Abstractions.Models.Enums;

namespace WebSmsNet.Abstractions.Models;

/// <summary>
/// Represents a request to send a text SMS.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class TextSmsSendRequest : SmsSendRequest
{
    /// <summary>
    /// Maximum number of SMS segments to be generated. If the system generates more than this number of SMS, the status code 4026 is returned. The default value of this parameter is 0.If set to 0, no limitation is applied.
    /// </summary>
    [JsonPropertyName("maxSmsPerMessage")]
    public int MaxSmsPerMessage { get; set; }

    /// <summary>
    /// The content of the SMS message (must be UTF-8 encoded).
    /// </summary>
    [JsonPropertyName("messageContent")]
    public required string MessageContent { get; set; }

    /// <summary>
    /// Type of message (default or voice).
    /// </summary>
    [JsonPropertyName("messageType")]
    public MessageType? MessageType { get; set; }
}
