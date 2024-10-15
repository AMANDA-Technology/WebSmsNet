using System.Diagnostics.CodeAnalysis;
using WebSmsNet.Abstractions;
using WebSmsNet.Abstractions.Configuration;
using WebSmsNet.Abstractions.Connectors;
using WebSmsNet.Connectors;

namespace WebSmsNet;

/// <inheritdoc />
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class WebSmsApiClient(WebSmsApiConnectionHandler connectionHandler) : IWebSmsApiClient
{
    /// <summary>
    /// Create WebSmsApiClient from http client
    /// </summary>
    /// <param name="httpClient"></param>
    public WebSmsApiClient(HttpClient httpClient)
        : this(new WebSmsApiConnectionHandler(httpClient))
    {
    }

    /// <summary>
    /// Create WebSmsApiClient from websms API options
    /// </summary>````````
    /// <param name="webSmsApiOptions"></param>
    public WebSmsApiClient(WebSmsApiOptions webSmsApiOptions)
        : this(new WebSmsApiConnectionHandler(new HttpClient().ApplyWebSmsApiOptions(webSmsApiOptions)))
    {
    }

    /// <inheritdoc />
    public IMessagingConnector Messaging { get; } = new MessagingConnector(connectionHandler);
}
