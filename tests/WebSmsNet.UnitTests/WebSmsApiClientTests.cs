using NSubstitute;
using NUnit.Framework;
using Shouldly;
using WebSmsNet.Abstractions.Configuration;
using WebSmsNet.Abstractions.Connectors;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.UnitTests.TestHelpers;

namespace WebSmsNet.UnitTests;

[TestFixture]
public class WebSmsApiClientTests
{
    private MockableHttpMessageHandler _httpMessageHandler = null!;
    private HttpClient _httpClient = null!;

    [SetUp]
    public void SetUp()
    {
        _httpMessageHandler = Substitute.For<MockableHttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandler)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
        _httpMessageHandler.Dispose();
    }

    [Test]
    public void Constructor_WithConnectionHandler_ExposesMessagingConnector()
    {
        // Arrange
        var handler = new WebSmsApiConnectionHandler(_httpClient);

        // Act
        var sut = new WebSmsApiClient(handler);

        // Assert
        sut.Messaging.ShouldNotBeNull();
        sut.Messaging.ShouldBeAssignableTo<IMessagingConnector>();
    }

    [Test]
    public void Constructor_WithHttpClient_ExposesMessagingConnector()
    {
        // Act
        var sut = new WebSmsApiClient(_httpClient);

        // Assert
        sut.Messaging.ShouldNotBeNull();
        sut.Messaging.ShouldBeAssignableTo<IMessagingConnector>();
    }

    [Test]
    public async Task Constructor_WithHttpClient_RoutesRequestsThroughThatClient()
    {
        // Arrange
        HttpRequestMessage? captured = null;
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                captured = call.Arg<HttpRequestMessage>();
                return Task.FromResult(HttpResponseFactory.JsonOk(HttpResponseFactory.SampleResponse()));
            });
        var sut = new WebSmsApiClient(_httpClient);
        var request = new TextSmsSendRequest
        {
            RecipientAddressList = ["4367612345678"],
            MessageContent = "hi"
        };

        // Act
        await sut.Messaging.SendTextMessage(request);

        // Assert
        captured.ShouldNotBeNull();
        captured!.RequestUri.ShouldBe(new Uri("https://api.example.com/rest/smsmessaging/text"));
    }

    [Test]
    public void Constructor_WithBearerOptions_ExposesMessagingConnector()
    {
        // Arrange
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Bearer,
            AccessToken = "test-token"
        };

        // Act
        var sut = new WebSmsApiClient(options);

        // Assert
        sut.Messaging.ShouldNotBeNull();
        sut.Messaging.ShouldBeAssignableTo<IMessagingConnector>();
    }

    [Test]
    public void Constructor_WithBasicOptions_ExposesMessagingConnector()
    {
        // Arrange
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Basic,
            Username = "user",
            Password = "pass"
        };

        // Act
        var sut = new WebSmsApiClient(options);

        // Assert
        sut.Messaging.ShouldNotBeNull();
        sut.Messaging.ShouldBeAssignableTo<IMessagingConnector>();
    }

    [Test]
    public void Constructor_WithBearerOptionsMissingToken_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Bearer,
            AccessToken = null
        };

        // Act
        var act = () => new WebSmsApiClient(options);

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }

    [Test]
    public void Constructor_WithBasicOptionsMissingCredentials_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Basic
        };

        // Act
        var act = () => new WebSmsApiClient(options);

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }

    [Test]
    public async Task Messaging_DelegatesPostsToProvidedConnectionHandler()
    {
        // Arrange
        var handler = Substitute.For<WebSmsApiConnectionHandler>(_httpClient);
        var expected = HttpResponseFactory.SampleResponse("delegated");
        handler.Post<MessageSendResponse>(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<CancellationToken>())
            .Returns(expected);
        var sut = new WebSmsApiClient(handler);
        var request = new TextSmsSendRequest
        {
            RecipientAddressList = ["4367612345678"],
            MessageContent = "hi"
        };

        // Act
        var result = await sut.Messaging.SendTextMessage(request);

        // Assert
        result.ShouldBe(expected);
        await handler.Received(1).Post<MessageSendResponse>(
            "/rest/smsmessaging/text",
            request,
            Arg.Any<CancellationToken>());
    }
}
