using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using WebSmsNet.Abstractions.Models.Enums;

namespace WebSmsNet.Abstractions.Models;

/// <summary>
/// Base class for all request types to send SMS messages.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public abstract class SmsSendRequest
{
    /// <summary>
    /// Optional. An identifier for the message defined by the client.
    /// Echoed in the <see cref="MessageSendResponse"/> and in delivery report webhooks.
    /// </summary>
    [JsonPropertyName("clientMessageId")]
    public string? ClientMessageId { get; set; }

    /// <summary>
    /// Optional. Content category used for categorizing the message
    /// (<see cref="Enums.ContentCategory.Informational"/> or <see cref="Enums.ContentCategory.Advertisement"/>).
    /// </summary>
    [JsonPropertyName("contentCategory")]
    public ContentCategory? ContentCategory { get; set; }

    /// <summary>
    /// Optional. URL to which delivery reports are forwarded.
    /// </summary>
    [JsonPropertyName("notificationCallbackUrl")]
    public string? NotificationCallbackUrl { get; set; }

    /// <summary>
    /// Optional. Priority of the message. Valid range is 1 to 9 (account-dependent).
    /// </summary>
    [JsonPropertyName("priority")]
    public int? Priority { get; set; }

    /// <summary>
    /// Required. List of recipients (E.164 formatted MSISDNs) to whom the message should be sent.
    /// See <see href="https://en.wikipedia.org/wiki/MSISDN">Wikipedia</see>.
    /// </summary>
    [JsonPropertyName("recipientAddressList")]
    public required List<string> RecipientAddressList { get; set; }

    /// <summary>
    /// Optional. Whether to send the message as a flash SMS. Defaults to <c>false</c> when omitted.
    /// </summary>
    [JsonPropertyName("sendAsFlashSms")]
    public bool? SendAsFlashSms { get; set; }

    /// <summary>
    /// Optional. Address of the sender (numeric MSISDN, alphanumeric, or shortcode depending on <see cref="SenderAddressType"/>).
    /// </summary>
    [JsonPropertyName("senderAddress")]
    public string? SenderAddress { get; set; }

    /// <summary>
    /// Optional. Type of the sender address
    /// (<see cref="AddressType.National"/>, <see cref="AddressType.International"/>,
    /// <see cref="AddressType.Alphanumeric"/>, or <see cref="AddressType.Shortcode"/>).
    /// </summary>
    [JsonPropertyName("senderAddressType")]
    public AddressType? SenderAddressType { get; set; }

    /// <summary>
    /// Optional. Whether the transmission is a test (simulated). Defaults to <c>false</c> when omitted.
    /// </summary>
    [JsonPropertyName("test")]
    public bool? Test { get; set; }

    /// <summary>
    /// Optional. Validity period in seconds for delivering the message.
    /// Valid range is 60 to 259200 (1 minute to 3 days).
    /// </summary>
    /// <remarks>
    /// The websms API misspells the JSON property name as <c>validityPeriode</c>; the C# property uses the correct spelling.
    /// </remarks>
    [JsonPropertyName("validityPeriode")]
    public int? ValidityPeriod { get; set; }
}
