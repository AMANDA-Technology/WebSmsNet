using WebSmsNet.Abstractions.Connectors;
using WebSmsNet.Abstractions.Models;

namespace WebSmsNet.Connectors;

/// <inheritdoc />
public class MessagingConnector(WebSmsApiConnectionHandler connectionHandler) : IMessagingConnector
{
    private const string MessagingApiBasePath = "/rest/smsmessaging";

    /// <inheritdoc />
    public async Task<MessageSendResponse> SendTextMessageAsync(TextSmsSendRequest request) =>
        await connectionHandler.PostAsync<MessageSendResponse>($"{MessagingApiBasePath}/text", request);

    /// <inheritdoc />
    public async Task<MessageSendResponse> SendBinaryMessageAsync(BinarySmsSendRequest request) =>
        await connectionHandler.PostAsync<MessageSendResponse>($"{MessagingApiBasePath}/binary", request);
}
