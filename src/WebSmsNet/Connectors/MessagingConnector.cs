using System.Runtime.InteropServices;
using WebSmsNet.Abstractions.Connectors;
using WebSmsNet.Abstractions.Models;

namespace WebSmsNet.Connectors;

/// <inheritdoc />
public class MessagingConnector(WebSmsApiConnectionHandler connectionHandler) : IMessagingConnector
{
    private const string MessagingApiBasePath = "/rest/smsmessaging";

    /// <inheritdoc />
    public async Task<MessageSendResponse> SendTextMessage(TextSmsSendRequest request, [Optional] CancellationToken cancellationToken) =>
        await connectionHandler.Post<MessageSendResponse>($"{MessagingApiBasePath}/text", request, cancellationToken);

    /// <inheritdoc />
    public async Task<MessageSendResponse> SendBinaryMessage(BinarySmsSendRequest request, [Optional] CancellationToken cancellationToken) =>
        await connectionHandler.Post<MessageSendResponse>($"{MessagingApiBasePath}/binary", request, cancellationToken);
}
