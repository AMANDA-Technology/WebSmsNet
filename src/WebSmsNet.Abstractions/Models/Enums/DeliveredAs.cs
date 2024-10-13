using System.Runtime.Serialization;

namespace WebSmsNet.Abstractions.Models.Enums;

/// <summary>
/// Delivery method used.
/// Possible values: "sms", "push", "failover-sms", "voice".
/// </summary>
public enum DeliveredAs
{
    /// <summary>
    /// Indicates that the notification was delivered as an SMS message.
    /// </summary>
    Sms,

    /// <summary>
    /// Indicates that the notification was delivered as a push message.
    /// </summary>
    Push,

    /// <summary>
    /// Indicates that the notification was delivered via failover to SMS.
    /// </summary>
    [EnumMember(Value = "failover-sms")]
    FailoverSms,

    /// <summary>
    /// Indicates that the notification is delivered through a voice call.
    /// </summary>
    Voice
}
