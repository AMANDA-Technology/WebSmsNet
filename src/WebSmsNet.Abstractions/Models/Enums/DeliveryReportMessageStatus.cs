namespace WebSmsNet.Abstractions.Models.Enums;

/// <summary>
/// Message status in the delivery report.
/// Possible values: "delivered", "undelivered", "expired", "deleted", "accepted", "rejected".
/// </summary>
public enum DeliveryReportMessageStatus
{
    /// <summary>
    /// Indicates that the message was successfully delivered to the recipient.
    /// </summary>
    Delivered,

    /// <summary>
    /// Indicates that the message was not successfully delivered to the recipient.
    /// </summary>
    Undelivered,

    /// <summary>
    /// Indicates that the message was not delivered because it expired before reaching the recipient.
    /// </summary>
    Expired,

    /// <summary>
    /// Indicates that the message has been deleted and was not delivered to the recipient.
    /// </summary>
    Deleted,

    /// <summary>
    /// Indicates that the message has been accepted for delivery but not yet delivered.
    /// </summary>
    Accepted,

    /// <summary>
    /// Indicates that the message was rejected and not delivered to the recipient.
    /// </summary>
    Rejected
}