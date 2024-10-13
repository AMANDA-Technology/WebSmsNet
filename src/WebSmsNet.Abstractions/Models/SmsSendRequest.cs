using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using WebSmsNet.Abstractions.Models.Enums;

namespace WebSmsNet.Abstractions.Models;

/// <summary>
/// Base class for all request types to send SMS messages.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public abstract class SmsSendRequest
{
    /// <summary>
    /// Optional client message ID.
    /// </summary>
    [JsonPropertyName("clientMessageId")]
    public string? ClientMessageId { get; set; }

    /// <summary>
    /// Content category used for categorizing the message (e.g., informational or advertisement).
    /// </summary>
    [JsonPropertyName("contentCategory")]
    public ContentCategory? ContentCategory { get; set; }

    /// <summary>
    /// URL to which delivery reports are forwarded.
    /// </summary>
    [JsonPropertyName("notificationCallbackUrl")]
    public string? NotificationCallbackUrl { get; set; }

    /// <summary>
    /// Priority of the message.
    /// </summary>
    [JsonPropertyName("priority")]
    public int? Priority { get; set; }

    /// <summary>
    /// List of recipients (E164 formatted MSISDNs) to whom the message should be sent.
    /// See <see href="https://en.wikipedia.org/wiki/MSISDN">Wikipedia</see>.
    /// </summary>
    [JsonPropertyName("recipientAddressList")]
    public required List<string> RecipientAddressList { get; set; }

    /// <summary>
    /// Whether to send the message as a flash SMS.
    /// </summary>
    [JsonPropertyName("sendAsFlashSms")]
    public bool SendAsFlashSms { get; set; }

    /// <summary>
    /// Address of the sender.
    /// </summary>
    [JsonPropertyName("senderAddress")]
    public string? SenderAddress { get; set; }

    /// <summary>
    /// Type of the sender address (e.g., national, international, alphanumeric, shortcode).
    /// </summary>
    [JsonPropertyName("senderAddressType")]
    public SenderAddressType? SenderAddressType { get; set; }

    /// <summary>
    /// Whether the transmission is a test (simulated).
    /// </summary>
    [JsonPropertyName("test")]
    public bool Test { get; set; }

    /// <summary>
    /// Validity period in seconds for delivering the message. A minimum of 1 minute and a maximum of 3 days are allowed. [ 60 .. 259200 ]
    /// </summary>
    [JsonPropertyName("validityPeriode")]
    public int? ValidityPeriod { get; set; }
}
