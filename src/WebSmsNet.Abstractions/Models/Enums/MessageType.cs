using System.Diagnostics.CodeAnalysis;

namespace WebSmsNet.Abstractions.Models.Enums;

/// <summary>
/// Represents a message type for sending voice messages.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum MessageType
{
    /// <summary>
    /// Represents the default message type.
    /// </summary>
    Default,

    /// <summary>
    /// Represents a message type for sending voice.
    /// </summary>
    Voice
}
