using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace WebSmsNet.Abstractions.Models;

/// <summary>
/// Represents a request to send a binary SMS.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class BinarySmsSendRequest : SmsSendRequest
{
    /// <summary>
    /// List of Base64 encoded binary message segments to be sent.
    /// </summary>
    [JsonPropertyName("messageContent")]
    public required List<string> MessageContent { get; set; }

    /// <summary>
    /// Whether a user data header is present in the message content.
    /// </summary>
    [JsonPropertyName("userDataHeaderPresent")]
    public bool UserDataHeaderPresent { get; set; }
}
