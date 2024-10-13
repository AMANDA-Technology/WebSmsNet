using System.Diagnostics.CodeAnalysis;

namespace WebSmsNet.Abstractions.Models.Enums;

/// <summary>
/// Defines the various types of sender addresses used in SMS messaging.
/// </summary>
/// <remarks>
/// The sender address type determines how the sender's address appears to the recipient.
/// </remarks>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum SenderAddressType
{
    /// <summary>
    /// Represents a sender address that is a national phone number.
    /// </summary>
    /// <remarks>
    /// The address must be a valid national number, adhering to the format and regulations of the sender's country.
    /// </remarks>
    National,

    /// <summary>
    /// Represents a sender address that is an international phone number.
    /// </summary>
    /// <remarks>
    /// The address must adhere to international dialing formats, which typically include the country code followed by the national number.
    /// </remarks>
    International,

    /// <summary>
    /// Represents a sender address that is alphanumeric.
    /// </summary>
    /// <remarks>
    /// The address can contain both letters and numbers, providing more flexibility and branding opportunities.
    /// </remarks>
    Alphanumeric,

    /// <summary>
    /// Represents a sender address that is a shortcode number.
    /// </summary>
    /// <remarks>
    /// Shortcode addresses are typically shorter than standard phone numbers and are often used for marketing campaigns, voting, and other services.
    /// </remarks>
    Shortcode
}
