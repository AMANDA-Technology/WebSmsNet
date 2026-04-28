using System.Net;
using System.Text.Json;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.Abstractions.Models.Enums;
using WebSmsNet.Abstractions.Serialization;
using WebSmsNet.UnitTests.TestHelpers;

namespace WebSmsNet.UnitTests;

[TestFixture]
public class WebSmsApiConnectionHandlerTests
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
    public async Task Post_ValidRequest_SendsPostToConfiguredEndpoint()
    {
        // Arrange
        HttpRequestMessage? captured = null;
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                captured = call.Arg<HttpRequestMessage>();
                return Task.FromResult(HttpResponseFactory.JsonOk(HttpResponseFactory.SampleResponse()));
            });
        var sut = new WebSmsApiConnectionHandler(_httpClient);

        // Act
        await sut.Post<MessageSendResponse>("/rest/smsmessaging/text", new { x = 1 });

        // Assert
        captured.ShouldNotBeNull();
        captured!.Method.ShouldBe(HttpMethod.Post);
        captured.RequestUri.ShouldBe(new Uri("https://api.example.com/rest/smsmessaging/text"));
    }

    [Test]
    public async Task Post_ValidRequest_SerializesBodyWithWebSmsJsonOptions()
    {
        // Arrange
        string? capturedBody = null;
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                var request = call.Arg<HttpRequestMessage>();
                capturedBody = await request.Content!.ReadAsStringAsync();
                return HttpResponseFactory.JsonOk(HttpResponseFactory.SampleResponse());
            });
        var sut = new WebSmsApiConnectionHandler(_httpClient);
        var payload = new TextSmsSendRequest
        {
            ClientMessageId = "abc",
            RecipientAddressList = ["4367612345678"],
            MessageContent = "Hello"
        };

        // Act
        await sut.Post<MessageSendResponse>("/rest/smsmessaging/text", payload);

        // Assert: camelCase property names, no nulls, validityPeriode misspelling absent (it's null and dropped)
        capturedBody.ShouldNotBeNull();
        capturedBody!.ShouldContain("\"clientMessageId\":\"abc\"");
        capturedBody.ShouldContain("\"recipientAddressList\":[\"4367612345678\"]");
        capturedBody.ShouldContain("\"messageContent\":\"Hello\"");
        capturedBody.ShouldNotContain("validityPeriode");
        capturedBody.ShouldNotContain("\"contentCategory\":null");
    }

    [Test]
    public async Task Post_ValidRequest_SetsJsonContentType()
    {
        // Arrange
        HttpRequestMessage? captured = null;
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                captured = call.Arg<HttpRequestMessage>();
                return Task.FromResult(HttpResponseFactory.JsonOk(HttpResponseFactory.SampleResponse()));
            });
        var sut = new WebSmsApiConnectionHandler(_httpClient);

        // Act
        await sut.Post<MessageSendResponse>("/rest/smsmessaging/text", new { });

        // Assert
        captured!.Content!.Headers.ContentType!.MediaType.ShouldBe("application/json");
    }

    [Test]
    public async Task Post_SuccessResponse_ReturnsDeserializedBody()
    {
        // Arrange
        var expected = HttpResponseFactory.SampleResponse("client-456", smsCount: 3);
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(_ => HttpResponseFactory.JsonOk(expected));
        var sut = new WebSmsApiConnectionHandler(_httpClient);

        // Act
        var result = await sut.Post<MessageSendResponse>("/rest/smsmessaging/text", new { });

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public async Task Post_NonSuccessStatusCode_ThrowsHttpRequestException()
    {
        // Arrange
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(_ => HttpResponseFactory.RawJson(HttpStatusCode.InternalServerError, "{}"));
        var sut = new WebSmsApiConnectionHandler(_httpClient);

        // Act
        var act = async () => await sut.Post<MessageSendResponse>("/rest/smsmessaging/text", new { });

        // Assert
        await act.ShouldThrowAsync<HttpRequestException>();
    }

    [Test]
    public async Task Post_ResponseBodyDeserializesToNull_ThrowsInvalidOperationException()
    {
        // Arrange
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(_ => HttpResponseFactory.RawJson(HttpStatusCode.OK, "null"));
        var sut = new WebSmsApiConnectionHandler(_httpClient);

        // Act
        var act = async () => await sut.Post<MessageSendResponse?>("/rest/smsmessaging/text", new { });

        // Assert
        var ex = await act.ShouldThrowAsync<InvalidOperationException>();
        ex.Message.ShouldBe("Failed to deserialize response.");
    }

    [Test]
    public async Task Post_PassesCancellationTokenToHttpClient()
    {
        // Arrange — HttpClient links the caller's token with its own timeout token, so we
        // verify cancellation propagation rather than reference equality.
        using var cts = new CancellationTokenSource();
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                var token = call.Arg<CancellationToken>();
                await cts.CancelAsync();
                token.ThrowIfCancellationRequested();
                return HttpResponseFactory.JsonOk(HttpResponseFactory.SampleResponse());
            });
        var sut = new WebSmsApiConnectionHandler(_httpClient);

        // Act
        var act = async () => await sut.Post<MessageSendResponse>("/rest/smsmessaging/text", new { }, cts.Token);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task Post_DefaultHooks_DoNotInterfereWithRequest()
    {
        // Arrange — use the base class with only default virtuals; success path must work end-to-end
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(_ => HttpResponseFactory.JsonOk(HttpResponseFactory.SampleResponse()));
        var sut = new WebSmsApiConnectionHandler(_httpClient);

        // Act
        var result = await sut.Post<MessageSendResponse>("/rest/smsmessaging/text", new { });

        // Assert
        result.StatusCode.ShouldBe(WebSmsStatusCode.Ok);
    }

    [Test]
    public async Task Post_OverriddenOnBeforePost_IsInvokedBeforeRequest()
    {
        // Arrange
        var sequence = new List<string>();
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                sequence.Add("send");
                return Task.FromResult(HttpResponseFactory.JsonOk(HttpResponseFactory.SampleResponse()));
            });
        var sut = new RecordingConnectionHandler(_httpClient, sequence);

        // Act
        await sut.Post<MessageSendResponse>("/rest/smsmessaging/text", new { x = 42 });

        // Assert
        sut.OnBeforePostCalls.ShouldHaveSingleItem();
        sut.OnBeforePostCalls[0].Endpoint.ShouldBe("/rest/smsmessaging/text");
        sut.OnBeforePostCalls[0].Data.ShouldNotBeNull();
        sequence[0].ShouldBe("before");
    }

    [Test]
    public async Task Post_OverriddenOnResponseReceived_IsInvokedAfterRequestAndBeforeEnsureSuccess()
    {
        // Arrange
        var sequence = new List<string>();
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                sequence.Add("send");
                return Task.FromResult(HttpResponseFactory.JsonOk(HttpResponseFactory.SampleResponse()));
            });
        var sut = new RecordingConnectionHandler(_httpClient, sequence);

        // Act
        await sut.Post<MessageSendResponse>("/rest/smsmessaging/text", new { });

        // Assert
        sequence.ShouldBe(["before", "send", "responseReceived", "ensureSuccess"]);
    }

    [Test]
    public async Task Post_OverriddenOnResponseReceived_CanReplaceResponse()
    {
        // Arrange
        var replacement = HttpResponseFactory.SampleResponse("replaced", smsCount: 7);
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(_ => HttpResponseFactory.RawJson(HttpStatusCode.OK, "{}"));
        var sut = new ReplacingResponseHandler(_httpClient, HttpResponseFactory.JsonOk(replacement));

        // Act
        var result = await sut.Post<MessageSendResponse>("/rest/smsmessaging/text", new { });

        // Assert
        result.ShouldBe(replacement);
    }

    [Test]
    public async Task Post_OverriddenEnsureSuccess_CanSuppressNonSuccessStatus()
    {
        // Arrange — server returns 500 but custom handler swallows it; body still deserializes
        var body = HttpResponseFactory.SampleResponse("survived");
        var responseJson = JsonSerializer.Serialize(body, WebSmsJsonSerialization.DefaultOptions);
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(_ => HttpResponseFactory.RawJson(HttpStatusCode.InternalServerError, responseJson));
        var sut = new SilentEnsureSuccessHandler(_httpClient);

        // Act
        var result = await sut.Post<MessageSendResponse>("/rest/smsmessaging/text", new { });

        // Assert
        result.ShouldBe(body);
    }

    [Test]
    public async Task Post_OverriddenSerializerOptions_AreUsedForRequestBody()
    {
        // Arrange
        string? capturedBody = null;
        _httpMessageHandler.SendAsyncMock(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                var request = call.Arg<HttpRequestMessage>();
                capturedBody = await request.Content!.ReadAsStringAsync();
                return HttpResponseFactory.JsonOk(HttpResponseFactory.SampleResponse());
            });
        var sut = new IndentedSerializerHandler(_httpClient);

        // Act
        await sut.Post<MessageSendResponse>("/rest/smsmessaging/text", new { hello = "world" });

        // Assert — indented JSON contains line breaks
        capturedBody.ShouldNotBeNull();
        capturedBody!.ShouldContain("\n");
    }

    private sealed class RecordingConnectionHandler(HttpClient httpClient, List<string> sequence)
        : WebSmsApiConnectionHandler(httpClient)
    {
        public List<(string Endpoint, object Data)> OnBeforePostCalls { get; } = [];

        protected override Func<string, object, CancellationToken, Task> OnBeforePost =>
            (endpoint, data, _) =>
            {
                OnBeforePostCalls.Add((endpoint, data));
                sequence.Add("before");
                return Task.CompletedTask;
            };

        protected override Func<HttpResponseMessage, CancellationToken, Task<HttpResponseMessage>> OnResponseReceived =>
            (response, _) =>
            {
                sequence.Add("responseReceived");
                return Task.FromResult(response);
            };

        protected override Action<HttpResponseMessage> EnsureSuccess => response =>
        {
            sequence.Add("ensureSuccess");
            response.EnsureSuccessStatusCode();
        };
    }

    private sealed class ReplacingResponseHandler(HttpClient httpClient, HttpResponseMessage replacement)
        : WebSmsApiConnectionHandler(httpClient)
    {
        protected override Func<HttpResponseMessage, CancellationToken, Task<HttpResponseMessage>> OnResponseReceived =>
            (_, _) => Task.FromResult(replacement);
    }

    private sealed class SilentEnsureSuccessHandler(HttpClient httpClient) : WebSmsApiConnectionHandler(httpClient)
    {
        protected override Action<HttpResponseMessage> EnsureSuccess => _ => { };
    }

    private sealed class IndentedSerializerHandler(HttpClient httpClient) : WebSmsApiConnectionHandler(httpClient)
    {
        protected override JsonSerializerOptions SerializerOptions { get; } = new(WebSmsJsonSerialization.DefaultOptions)
        {
            WriteIndented = true
        };
    }
}
