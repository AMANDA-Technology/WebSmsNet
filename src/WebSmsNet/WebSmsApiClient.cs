using WebSmsNet.Abstractions;
using WebSmsNet.Abstractions.Connectors;
using WebSmsNet.Connectors;

namespace WebSmsNet;

/// <inheritdoc />
public class WebSmsApiClient(WebSmsApiConnectionHandler connectionHandler) : IWebSmsApiClient
{
    /// <inheritdoc />
    public IMessagingConnector Messaging { get; } = new MessagingConnector(connectionHandler);
}
