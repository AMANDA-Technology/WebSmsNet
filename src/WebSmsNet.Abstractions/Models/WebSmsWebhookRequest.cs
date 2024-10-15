using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using WebSmsNet.Abstractions.Models.Enums;

namespace WebSmsNet.Abstractions.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Webhook request class for websms services.
/// </summary>
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
public static class WebSmsWebhookRequest
{
    /// <summary>
    /// Base class for websms webhook requests
    /// </summary>
    public abstract class Base
    {
        /// <summary>
        /// Defines the content type of your notification.
        /// Possible values: "text", "binary", "deliveryReport".
        /// MessageTypes: text, binary, deliveryReport
        /// Example: "text"
        /// </summary>
        [JsonPropertyName("messageType")]
        public required WebhookMessageType MessageType { get; init; }

        /// <summary>
        /// 20-digit identifiFcation of your notification.
        /// MessageTypes: text, binary, deliveryReport
        /// Example: "050f9005180a2a212469"
        /// </summary>
        [JsonPropertyName("notificationId")]
        public required string NotificationId { get; init; }

        /// <summary>
        /// Originator of the sender, e.g., "4366012345678".
        /// MessageTypes: text, binary, deliveryReport
        /// Example: "4366012345678"
        /// </summary>
        [JsonPropertyName("senderAddress")]
        public required string SenderAddress { get; init; }
    }

    /// <summary>
    /// Base class for text and binary message types in websms webhook requests.
    /// </summary>
    public abstract class TextAndBinaryBase : Base
    {
        /// <summary>
        /// Indicates whether the received message is an SMS or a flash-SMS.
        /// MessageTypes: text, binary
        /// Example: true
        /// </summary>
        [JsonPropertyName("messageFlashSms")]
        public bool MessageFlashSms { get; init; }

        /// <summary>
        /// Defines the number format of the mobile originated senderAddress.
        /// MessageTypes: text, binary
        /// Example: AddressType.International
        /// </summary>
        [JsonPropertyName("senderAddressType")]
        public required AddressType SenderAddressType { get; init; }

        /// <summary>
        /// Sender's address, can be international, national, or a shortcode.
        /// MessageTypes: text, binary, deliveryReport
        /// Example: "066012345678"
        /// </summary>
        [JsonPropertyName("recipientAddress")]
        public required string RecipientAddress { get; init; }

        /// <summary>
        /// Defines the number format of the mobile originated message.
        /// MessageTypes: text, binary
        /// Possible values: "international", "national", "shortcode"
        /// Example: AddressType.National
        /// </summary>
        [JsonPropertyName("recipientAddressType")]
        public required AddressType RecipientAddressType { get; init; }
    }

    /// <summary>
    /// Represents a text message in a webhook request.
    /// </summary>
    public class Text : TextAndBinaryBase
    {
        /// <summary>
        /// Text body of the message encoded in UTF-8.
        /// In case of concatenated SMS, it contains the complete content of all segments.
        /// MessageTypes: text
        /// Example: "Hello World!"
        /// </summary>
        [JsonPropertyName("textMessageContent")]
        public required string TextMessageContent { get; init; }
    }

    /// <summary>
    /// Represents a binary message in a webhook request.
    /// </summary>
    public class Binary : TextAndBinaryBase
    {
        /// <summary>
        /// Indicates whether a user-data-header is included within a Base64 encoded byte segment.
        /// MessageTypes: binary
        /// Example: true
        /// </summary>
        [JsonPropertyName("userDataHeaderPresent")]
        public required bool UserDataHeaderPresent { get; init; }

        /// <summary>
        /// Content of a binary SMS as an array of Base64 strings (URL safe).
        /// MessageTypes: binary
        /// </summary>
        [JsonPropertyName("binaryMessageContent")]
        public required List<string> BinaryMessageContent { get; init; }
    }

    /// <summary>
    /// Represents a delivery report for a message sent via websms services.
    /// </summary>
    public class DeliveryReport : Base
    {
        /// <summary>
        /// Unique transfer-id to connect the deliveryReport to the initial message.
        /// MessageTypes: deliveryReport
        /// Example: "0051949fe700053c4615"
        /// </summary>
        [JsonPropertyName("transferId")]
        public required string TransferId { get; init; }

        /// <summary>
        /// Message status in the delivery report.
        /// Possible values: "delivered", "undelivered", "expired", "deleted", "accepted", "rejected".
        /// MessageTypes: deliveryReport
        /// Example: DeliveryReportMessageStatus.Delivered
        /// </summary>
        [JsonPropertyName("deliveryReportMessageStatus")]
        public required DeliveryReportMessageStatus DeliveryReportMessageStatus { get; init; }

        /// <summary>
        /// Time when the message was sent to the recipient's address.
        /// ISO 8601 timestamp, e.g., "2013-05-27T13:36:00.000+02:00".
        /// MessageTypes: deliveryReport
        /// Example: 2013-05-27T13:36:00.000+02:00
        /// </summary>
        [JsonPropertyName("sentOn")]
        public required DateTimeOffset SentOn { get; init; }

        /// <summary>
        /// Time when the message was submitted to the mobile operator's network.
        /// ISO 8601 timestamp, e.g., "2013-05-27T13:36:00.000+02:00".
        /// MessageTypes: deliveryReport
        /// Example: 2013-05-27T13:36:00.000+02:00
        /// </summary>
        [JsonPropertyName("deliveredOn")]
        public required DateTimeOffset DeliveredOn { get; init; }

        /// <summary>
        /// Delivery method used.
        /// Possible values: "sms", "push", "failover-sms", "voice".
        /// MessageTypes: deliveryReport
        /// Example: DeliveredAs.Sms
        /// </summary>
        [JsonPropertyName("deliveredAs")]
        public DeliveredAs? DeliveredAs { get; init; }

        /// <summary>
        /// In the case of a delivery report, contains the optional submitted message ID.
        /// MessageTypes: deliveryReport
        /// </summary>
        [JsonPropertyName("clientMessageId")]
        public required string ClientMessageId { get; init; }
    }
}
