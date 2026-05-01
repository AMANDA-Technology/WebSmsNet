using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace WebSmsNet.Abstractions.Models;

/// <summary>
/// Represents a request to send a binary SMS via the <c>/rest/smsmessaging/binary</c> endpoint.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class BinarySmsSendRequest : SmsSendRequest
{
    /// <summary>
    /// Required. Ordered list of Base64-encoded binary segments that together form the message.
    /// Each entry corresponds to one SMS segment on the wire.
    /// When <see cref="UserDataHeaderPresent"/> is <c>true</c>, every segment must already
    /// include its own User Data Header (UDH); otherwise the API generates the UDH for
    /// concatenated SMS automatically.
    /// </summary>
    [JsonPropertyName("messageContent")]
    public required List<string> MessageContent { get; set; }

    /// <summary>
    /// Optional. Indicates whether each entry in <see cref="MessageContent"/> already
    /// contains a User Data Header (UDH). Defaults to <c>false</c> when omitted, in which case
    /// the API generates the UDH for concatenated SMS.
    /// </summary>
    [JsonPropertyName("userDataHeaderPresent")]
    public bool? UserDataHeaderPresent { get; set; }
}
