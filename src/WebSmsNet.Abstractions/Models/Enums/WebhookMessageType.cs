namespace WebSmsNet.Abstractions.Models.Enums;

/// <summary>
/// Defines the content type of your notification.
/// Possible values: "text", "binary", "deliveryReport".
/// </summary>
public enum WebhookMessageType
{
    /// <summary>
    /// Indicates that the notification contains a text message.
    /// </summary>
    Text,

    /// <summary>
    /// Indicates that the notification contains a binary message.
    /// </summary>
    Binary,

    /// <summary>
    /// Indicates that the notification contains a delivery report.
    /// </summary>
    DeliveryReport
}
