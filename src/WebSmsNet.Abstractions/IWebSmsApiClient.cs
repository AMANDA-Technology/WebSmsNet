using System.Diagnostics.CodeAnalysis;
using WebSmsNet.Abstractions.Connectors;

namespace WebSmsNet.Abstractions;

/// <summary>
/// Client to interact with the websms messaging API
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface IWebSmsApiClient
{
    /// <summary>
    /// Provides access to the messaging functionalities of the websms API, allowing you
    /// to send both text and binary SMS messages.
    /// </summary>
    public IMessagingConnector Messaging { get; }
}
