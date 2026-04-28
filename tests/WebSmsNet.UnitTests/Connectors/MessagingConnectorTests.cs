using NSubstitute;
using NUnit.Framework;
using Shouldly;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.Connectors;
using WebSmsNet.UnitTests.TestHelpers;

namespace WebSmsNet.UnitTests.Connectors;

[TestFixture]
public class MessagingConnectorTests
{
    private MockableHttpMessageHandler _httpMessageHandler = null!;
    private HttpClient _httpClient = null!;
    private WebSmsApiConnectionHandler _connectionHandler = null!;

    [SetUp]
    public void SetUp()
    {
        _httpMessageHandler = Substitute.For<MockableHttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandler)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        _connectionHandler = Substitute.For<WebSmsApiConnectionHandler>(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
        _httpMessageHandler.Dispose();
    }

    [Test]
    public async Task SendTextMessage_PostsToTextEndpoint()
    {
        // Arrange
        _connectionHandler.Post<MessageSendResponse>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(HttpResponseFactory.SampleResponse());
        var sut = new MessagingConnector(_connectionHandler);
        var request = new TextSmsSendRequest
        {
            RecipientAddressList = ["4367612345678"],
            MessageContent = "hi"
        };

        // Act
        await sut.SendTextMessage(request);

        // Assert
        await _connectionHandler.Received(1).Post<MessageSendResponse>(
            "/rest/smsmessaging/text",
            request,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SendTextMessage_PassesRequestBodyUnchanged()
    {
        // Arrange
        object? capturedBody = null;
        _connectionHandler.Post<MessageSendResponse>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                capturedBody = call.Arg<object>();
                return HttpResponseFactory.SampleResponse();
            });
        var sut = new MessagingConnector(_connectionHandler);
        var request = new TextSmsSendRequest
        {
            RecipientAddressList = ["4367612345678"],
            MessageContent = "hi",
            ClientMessageId = "abc"
        };

        // Act
        await sut.SendTextMessage(request);

        // Assert
        capturedBody.ShouldBeSameAs(request);
    }

    [Test]
    public async Task SendTextMessage_ReturnsConnectionHandlerResponse()
    {
        // Arrange
        var expected = HttpResponseFactory.SampleResponse("text-msg-id", smsCount: 1);
        _connectionHandler.Post<MessageSendResponse>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(expected);
        var sut = new MessagingConnector(_connectionHandler);

        // Act
        var result = await sut.SendTextMessage(new TextSmsSendRequest
        {
            RecipientAddressList = ["4367612345678"],
            MessageContent = "hi"
        });

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public async Task SendTextMessage_PropagatesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        _connectionHandler.Post<MessageSendResponse>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                capturedToken = call.Arg<CancellationToken>();
                return HttpResponseFactory.SampleResponse();
            });
        var sut = new MessagingConnector(_connectionHandler);

        // Act
        await sut.SendTextMessage(new TextSmsSendRequest
        {
            RecipientAddressList = ["4367612345678"],
            MessageContent = "hi"
        }, cts.Token);

        // Assert
        capturedToken.ShouldBe(cts.Token);
    }

    [Test]
    public async Task SendBinaryMessage_PostsToBinaryEndpoint()
    {
        // Arrange
        _connectionHandler.Post<MessageSendResponse>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(HttpResponseFactory.SampleResponse());
        var sut = new MessagingConnector(_connectionHandler);
        var request = new BinarySmsSendRequest
        {
            RecipientAddressList = ["4367612345678"],
            MessageContent = ["aGVsbG8="]
        };

        // Act
        await sut.SendBinaryMessage(request);

        // Assert
        await _connectionHandler.Received(1).Post<MessageSendResponse>(
            "/rest/smsmessaging/binary",
            request,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SendBinaryMessage_PassesRequestBodyUnchanged()
    {
        // Arrange
        object? capturedBody = null;
        _connectionHandler.Post<MessageSendResponse>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                capturedBody = call.Arg<object>();
                return HttpResponseFactory.SampleResponse();
            });
        var sut = new MessagingConnector(_connectionHandler);
        var request = new BinarySmsSendRequest
        {
            RecipientAddressList = ["4367612345678"],
            MessageContent = ["aGVsbG8="],
            UserDataHeaderPresent = true
        };

        // Act
        await sut.SendBinaryMessage(request);

        // Assert
        capturedBody.ShouldBeSameAs(request);
    }

    [Test]
    public async Task SendBinaryMessage_ReturnsConnectionHandlerResponse()
    {
        // Arrange
        var expected = HttpResponseFactory.SampleResponse("binary-msg-id", smsCount: 3);
        _connectionHandler.Post<MessageSendResponse>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(expected);
        var sut = new MessagingConnector(_connectionHandler);

        // Act
        var result = await sut.SendBinaryMessage(new BinarySmsSendRequest
        {
            RecipientAddressList = ["4367612345678"],
            MessageContent = ["aGVsbG8="]
        });

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public async Task SendBinaryMessage_PropagatesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        _connectionHandler.Post<MessageSendResponse>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                capturedToken = call.Arg<CancellationToken>();
                return HttpResponseFactory.SampleResponse();
            });
        var sut = new MessagingConnector(_connectionHandler);

        // Act
        await sut.SendBinaryMessage(new BinarySmsSendRequest
        {
            RecipientAddressList = ["4367612345678"],
            MessageContent = ["aGVsbG8="]
        }, cts.Token);

        // Assert
        capturedToken.ShouldBe(cts.Token);
    }
}
