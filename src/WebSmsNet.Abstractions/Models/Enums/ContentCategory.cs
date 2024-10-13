using System.Diagnostics.CodeAnalysis;

namespace WebSmsNet.Abstractions.Models.Enums;

/// <summary>
/// Specifies the category of the content for an SMS message.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum ContentCategory
{
    /// <summary>
    /// Represents content that is intended to convey information.
    /// Used to classify messages that provide updates, alerts, or other informational content.
    /// </summary>
    Informational,

    /// <summary>
    /// Represents the category for content that is intended for promotional or marketing purposes.
    /// </summary>
    Advertisement
}
