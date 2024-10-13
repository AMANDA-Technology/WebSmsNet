namespace WebSmsNet.Abstractions.Models.Enums;

/// <summary>
/// Represents the status codes returned by the WebSMS API.
/// </summary>
public enum WebSmsStatusCode
{
    /// <summary>
    /// Request accepted, Message(s) sent.
    /// </summary>
    Ok = 2000,

    /// <summary>
    /// Request accepted, Message(s) queued.
    /// </summary>
    OkQueued = 2001,

    /// <summary>
    /// Invalid Credentials. Inactive account or customer.
    /// </summary>
    InvalidCredentials = 4001,

    /// <summary>
    /// One or more recipients are not in the correct format or containing invalid MSISDNs.
    /// </summary>
    InvalidRecipient = 4002,

    /// <summary>
    /// Invalid Sender. Sender address or type is invalid.
    /// </summary>
    InvalidSender = 4003,

    /// <summary>
    /// Invalid messageType.
    /// </summary>
    InvalidMessageType = 4004,

    /// <summary>
    /// Invalid clientMessageId.
    /// </summary>
    InvalidMessageId = 4008,

    /// <summary>
    /// Message text (messageContent) is invalid.
    /// </summary>
    InvalidText = 4009,

    /// <summary>
    /// Message limit is reached.
    /// </summary>
    MsgLimitExceeded = 4013,

    /// <summary>
    /// Sender IP address is not authorized.
    /// </summary>
    UnauthorizedIp = 4014,

    /// <summary>
    /// Invalid Message Priority.
    /// </summary>
    InvalidMessagePriority = 4015,

    /// <summary>
    /// Invalid notificationCallbackUrl.
    /// </summary>
    InvalidCodReturnAddress = 4016,

    /// <summary>
    /// A required parameter was not given. The parameter name is shown in the statusMessage.
    /// </summary>
    ParameterMissing = 4019,

    /// <summary>
    /// Account is invalid.
    /// </summary>
    InvalidAccount = 4021,

    /// <summary>
    /// Access to the API was denied.
    /// </summary>
    AccessDenied = 4022,

    /// <summary>
    /// Request limit exceeded for this IP address.
    /// </summary>
    ThrottlingSpammingIp = 4023,

    /// <summary>
    /// Transfer rate for immediate transmissions exceeded. Too many recipients in this request (1000).
    /// </summary>
    ThrottlingTooManyRecipients = 4025,

    /// <summary>
    /// The message content results in too many (automatically generated) SMS segments.
    /// </summary>
    MaxSmsPerMessageExceeded = 4026,

    /// <summary>
    /// A messageContent segment is invalid.
    /// </summary>
    InvalidMessageSegment = 4027,

    /// <summary>
    /// Recipients not allowed.
    /// </summary>
    RecipientsNotAllowed = 4029,

    /// <summary>
    /// All recipients blacklisted.
    /// </summary>
    RecipientsBlacklisted = 4031,

    /// <summary>
    /// Not allowed to send SMS messages.
    /// </summary>
    SmsDisabled = 4035,

    /// <summary>
    /// Invalid contentCategory.
    /// </summary>
    InvalidContentCategory = 4040,

    /// <summary>
    /// Invalid validityPeriod.
    /// </summary>
    InvalidValidityPeriod = 4041,

    /// <summary>
    /// All the recipients are blocked by quality rating.
    /// </summary>
    RecipientsBlockedByQualityRating = 4042,

    /// <summary>
    /// All the recipients are blocked by spam-check.
    /// </summary>
    RecipientsBlockedBySpamCheck = 4043,

    /// <summary>
    /// Internal error.
    /// </summary>
    InternalError = 5000,

    /// <summary>
    /// Service unavailable.
    /// </summary>
    ServiceUnavailable = 5003
}
