using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Shouldly;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.Abstractions.Models.Enums;
using WebSmsNet.Abstractions.Serialization;
using WebSmsNet.Connectors;
using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace WebSmsNet.IntegrationTests;

[TestFixture]
public class MessagingConnectorIntegrationTests
{
    private const string BearerToken = "test-token";

    private WireMockServer _server = null!;
    private HttpClient _httpClient = null!;
    private MessagingConnector _connector = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _server = WireMockServer.Start();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _server.Stop();
        _server.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        _server.Reset();

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_server.Url!)
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

        _connector = new MessagingConnector(new WebSmsApiConnectionHandler(_httpClient));
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    private IRequestMessage SingleRequest() =>
        _server.LogEntries.Single().RequestMessage.ShouldNotBeNull();

    [Test]
    public async Task SendTextMessage_WithValidRequest_HitsTextEndpointAndReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new MessageSendResponse
        {
            ClientMessageId = "client-msg-1",
            SmsCount = 1,
            StatusCode = WebSmsStatusCode.Ok,
            StatusMessage = "OK",
            TransferId = "transfer-1"
        };

        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/text").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(expectedResponse, WebSmsJsonSerialization.DefaultOptions)));

        var request = new TextSmsSendRequest
        {
            ClientMessageId = "client-msg-1",
            RecipientAddressList = ["4367612345678"],
            MessageContent = "hello"
        };

        // Act
        var response = await _connector.SendTextMessage(request);

        // Assert
        response.ShouldBe(expectedResponse);

        var requestMessage = SingleRequest();
        requestMessage.AbsolutePath.ShouldBe("/rest/smsmessaging/text");
        requestMessage.Method.ShouldBe("POST");
    }

    [Test]
    public async Task SendTextMessage_SerializesJsonBodyWithCamelCaseAndExpectedFields()
    {
        // Arrange
        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/text").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new MessageSendResponse
                {
                    ClientMessageId = "id-1",
                    SmsCount = 1,
                    StatusCode = WebSmsStatusCode.Ok,
                    StatusMessage = "OK",
                    TransferId = "tx-1"
                }, WebSmsJsonSerialization.DefaultOptions)));

        var request = new TextSmsSendRequest
        {
            ClientMessageId = "id-1",
            RecipientAddressList = ["4367612345678"],
            MessageContent = "hello world",
            Test = true,
            ValidityPeriod = 600
        };

        // Act
        await _connector.SendTextMessage(request);

        // Assert
        var body = SingleRequest().Body.ShouldNotBeNull();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.GetProperty("clientMessageId").GetString().ShouldBe("id-1");
        root.GetProperty("messageContent").GetString().ShouldBe("hello world");
        root.GetProperty("recipientAddressList").EnumerateArray().Single().GetString().ShouldBe("4367612345678");
        root.GetProperty("test").GetBoolean().ShouldBeTrue();
        // websms misspells the property name "validityPeriode" — keep that on the wire.
        root.GetProperty("validityPeriode").GetInt32().ShouldBe(600);
    }

    [Test]
    public async Task SendTextMessage_OmitsUnsetOptionalFieldsFromJsonBody()
    {
        // Arrange — only required fields populated.
        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/text").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new MessageSendResponse
                {
                    ClientMessageId = null,
                    SmsCount = 1,
                    StatusCode = WebSmsStatusCode.Ok,
                    StatusMessage = "OK",
                    TransferId = "tx-min"
                }, WebSmsJsonSerialization.DefaultOptions)));

        var request = new TextSmsSendRequest
        {
            RecipientAddressList = ["4367612345678"],
            MessageContent = "minimal"
        };

        // Act
        await _connector.SendTextMessage(request);

        // Assert — none of the documented optional fields should appear when they are unset.
        var body = SingleRequest().Body.ShouldNotBeNull();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("clientMessageId", out _).ShouldBeFalse();
        root.TryGetProperty("contentCategory", out _).ShouldBeFalse();
        root.TryGetProperty("notificationCallbackUrl", out _).ShouldBeFalse();
        root.TryGetProperty("priority", out _).ShouldBeFalse();
        root.TryGetProperty("sendAsFlashSms", out _).ShouldBeFalse();
        root.TryGetProperty("senderAddress", out _).ShouldBeFalse();
        root.TryGetProperty("senderAddressType", out _).ShouldBeFalse();
        root.TryGetProperty("test", out _).ShouldBeFalse();
        root.TryGetProperty("validityPeriode", out _).ShouldBeFalse();
        root.TryGetProperty("maxSmsPerMessage", out _).ShouldBeFalse();
        root.TryGetProperty("messageType", out _).ShouldBeFalse();

        root.GetProperty("messageContent").GetString().ShouldBe("minimal");
        root.GetProperty("recipientAddressList").EnumerateArray().Single().GetString().ShouldBe("4367612345678");
    }

    [Test]
    public async Task SendTextMessage_SerializesAllDocumentedOptionalFieldsWithCorrectJsonNamesAndCasing()
    {
        // Arrange
        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/text").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new MessageSendResponse
                {
                    ClientMessageId = "client-full",
                    SmsCount = 1,
                    StatusCode = WebSmsStatusCode.Ok,
                    StatusMessage = "OK",
                    TransferId = "tx-full"
                }, WebSmsJsonSerialization.DefaultOptions)));

        var request = new TextSmsSendRequest
        {
            ClientMessageId = "client-full",
            ContentCategory = ContentCategory.Advertisement,
            NotificationCallbackUrl = "https://example.test/cb",
            Priority = 9,
            RecipientAddressList = ["4367612345678"],
            SendAsFlashSms = false,
            SenderAddress = "4367600000",
            SenderAddressType = AddressType.International,
            Test = true,
            ValidityPeriod = 259200,
            MaxSmsPerMessage = 1,
            MessageContent = "all fields",
            MessageType = MessageType.Voice
        };

        // Act
        await _connector.SendTextMessage(request);

        // Assert
        var body = SingleRequest().Body.ShouldNotBeNull();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.GetProperty("clientMessageId").GetString().ShouldBe("client-full");
        root.GetProperty("contentCategory").GetString().ShouldBe("advertisement");
        root.GetProperty("notificationCallbackUrl").GetString().ShouldBe("https://example.test/cb");
        root.GetProperty("priority").GetInt32().ShouldBe(9);
        root.GetProperty("recipientAddressList").EnumerateArray().Single().GetString().ShouldBe("4367612345678");
        root.GetProperty("sendAsFlashSms").GetBoolean().ShouldBeFalse();
        root.GetProperty("senderAddress").GetString().ShouldBe("4367600000");
        root.GetProperty("senderAddressType").GetString().ShouldBe("international");
        root.GetProperty("test").GetBoolean().ShouldBeTrue();
        root.GetProperty("validityPeriode").GetInt32().ShouldBe(259200);
        root.GetProperty("maxSmsPerMessage").GetInt32().ShouldBe(1);
        root.GetProperty("messageContent").GetString().ShouldBe("all fields");
        root.GetProperty("messageType").GetString().ShouldBe("voice");
    }

    [Test]
    public async Task SendTextMessage_SendsBearerAuthorizationHeader()
    {
        // Arrange
        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/text").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new MessageSendResponse
                {
                    ClientMessageId = "id",
                    SmsCount = 1,
                    StatusCode = WebSmsStatusCode.Ok,
                    StatusMessage = "OK",
                    TransferId = "tx"
                }, WebSmsJsonSerialization.DefaultOptions)));

        var request = new TextSmsSendRequest
        {
            ClientMessageId = "id",
            RecipientAddressList = ["4367612345678"],
            MessageContent = "x"
        };

        // Act
        await _connector.SendTextMessage(request);

        // Assert
        var headers = SingleRequest().Headers.ShouldNotBeNull();
        headers["Authorization"].ToString().ShouldBe($"Bearer {BearerToken}");
    }

    [Test]
    public async Task SendBinaryMessage_WithValidRequest_HitsBinaryEndpointAndReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new MessageSendResponse
        {
            ClientMessageId = "binary-msg-1",
            SmsCount = 3,
            StatusCode = WebSmsStatusCode.OkQueued,
            StatusMessage = "Queued",
            TransferId = "transfer-bin-1"
        };

        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/binary").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(expectedResponse, WebSmsJsonSerialization.DefaultOptions)));

        var request = new BinarySmsSendRequest
        {
            ClientMessageId = "binary-msg-1",
            RecipientAddressList = ["4367612345678"],
            MessageContent = ["AQID", "BAUG", "BwgJ"],
            UserDataHeaderPresent = true
        };

        // Act
        var response = await _connector.SendBinaryMessage(request);

        // Assert
        response.ShouldBe(expectedResponse);

        var requestMessage = SingleRequest();
        requestMessage.AbsolutePath.ShouldBe("/rest/smsmessaging/binary");
        requestMessage.Method.ShouldBe("POST");
    }

    [Test]
    public async Task SendBinaryMessage_SerializesJsonBodyWithCamelCaseAndExpectedFields()
    {
        // Arrange
        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/binary").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new MessageSendResponse
                {
                    ClientMessageId = "bin-id",
                    SmsCount = 3,
                    StatusCode = WebSmsStatusCode.Ok,
                    StatusMessage = "OK",
                    TransferId = "tx-bin"
                }, WebSmsJsonSerialization.DefaultOptions)));

        var request = new BinarySmsSendRequest
        {
            ClientMessageId = "bin-id",
            RecipientAddressList = ["4367612345678"],
            MessageContent = ["AQID", "BAUG"],
            UserDataHeaderPresent = true
        };

        // Act
        await _connector.SendBinaryMessage(request);

        // Assert
        var body = SingleRequest().Body.ShouldNotBeNull();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.GetProperty("clientMessageId").GetString().ShouldBe("bin-id");
        root.GetProperty("userDataHeaderPresent").GetBoolean().ShouldBeTrue();

        var content = root.GetProperty("messageContent").EnumerateArray().Select(e => e.GetString()).ToList();
        content.ShouldBe(["AQID", "BAUG"]);
    }

    [Test]
    public async Task SendBinaryMessage_OmitsUnsetOptionalFieldsFromJsonBody()
    {
        // Arrange — only required fields populated.
        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/binary").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new MessageSendResponse
                {
                    ClientMessageId = null,
                    SmsCount = 1,
                    StatusCode = WebSmsStatusCode.Ok,
                    StatusMessage = "OK",
                    TransferId = "tx-bin-min"
                }, WebSmsJsonSerialization.DefaultOptions)));

        var request = new BinarySmsSendRequest
        {
            RecipientAddressList = ["4367612345678"],
            MessageContent = ["AQID"]
        };

        // Act
        await _connector.SendBinaryMessage(request);

        // Assert — none of the documented optional fields should appear when they are unset.
        var body = SingleRequest().Body.ShouldNotBeNull();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("clientMessageId", out _).ShouldBeFalse();
        root.TryGetProperty("contentCategory", out _).ShouldBeFalse();
        root.TryGetProperty("notificationCallbackUrl", out _).ShouldBeFalse();
        root.TryGetProperty("priority", out _).ShouldBeFalse();
        root.TryGetProperty("sendAsFlashSms", out _).ShouldBeFalse();
        root.TryGetProperty("senderAddress", out _).ShouldBeFalse();
        root.TryGetProperty("senderAddressType", out _).ShouldBeFalse();
        root.TryGetProperty("test", out _).ShouldBeFalse();
        root.TryGetProperty("validityPeriode", out _).ShouldBeFalse();
        root.TryGetProperty("userDataHeaderPresent", out _).ShouldBeFalse();
        // The binary endpoint does not accept maxSmsPerMessage or messageType.
        root.TryGetProperty("maxSmsPerMessage", out _).ShouldBeFalse();
        root.TryGetProperty("messageType", out _).ShouldBeFalse();

        root.GetProperty("messageContent").EnumerateArray().Single().GetString().ShouldBe("AQID");
        root.GetProperty("recipientAddressList").EnumerateArray().Single().GetString().ShouldBe("4367612345678");
    }

    [Test]
    public async Task SendBinaryMessage_SerializesAllDocumentedOptionalFieldsWithCorrectJsonNamesAndCasing()
    {
        // Arrange
        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/binary").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new MessageSendResponse
                {
                    ClientMessageId = "bin-full",
                    SmsCount = 2,
                    StatusCode = WebSmsStatusCode.Ok,
                    StatusMessage = "OK",
                    TransferId = "tx-bin-full"
                }, WebSmsJsonSerialization.DefaultOptions)));

        var request = new BinarySmsSendRequest
        {
            ClientMessageId = "bin-full",
            ContentCategory = ContentCategory.Advertisement,
            NotificationCallbackUrl = "https://example.test/cb",
            Priority = 9,
            RecipientAddressList = ["4367612345678"],
            SendAsFlashSms = false,
            SenderAddress = "4367600000",
            SenderAddressType = AddressType.International,
            Test = true,
            ValidityPeriod = 259200,
            MessageContent = ["AQID", "BAUG"],
            UserDataHeaderPresent = true
        };

        // Act
        await _connector.SendBinaryMessage(request);

        // Assert
        var body = SingleRequest().Body.ShouldNotBeNull();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.GetProperty("clientMessageId").GetString().ShouldBe("bin-full");
        root.GetProperty("contentCategory").GetString().ShouldBe("advertisement");
        root.GetProperty("notificationCallbackUrl").GetString().ShouldBe("https://example.test/cb");
        root.GetProperty("priority").GetInt32().ShouldBe(9);
        root.GetProperty("recipientAddressList").EnumerateArray().Single().GetString().ShouldBe("4367612345678");
        root.GetProperty("sendAsFlashSms").GetBoolean().ShouldBeFalse();
        root.GetProperty("senderAddress").GetString().ShouldBe("4367600000");
        root.GetProperty("senderAddressType").GetString().ShouldBe("international");
        root.GetProperty("test").GetBoolean().ShouldBeTrue();
        root.GetProperty("validityPeriode").GetInt32().ShouldBe(259200);
        root.GetProperty("messageContent").EnumerateArray().Select(e => e.GetString()).ToList()
            .ShouldBe(["AQID", "BAUG"]);
        root.GetProperty("userDataHeaderPresent").GetBoolean().ShouldBeTrue();
    }

    [Test]
    public async Task SendBinaryMessage_SendsBearerAuthorizationHeader()
    {
        // Arrange
        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/binary").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new MessageSendResponse
                {
                    ClientMessageId = "id",
                    SmsCount = 1,
                    StatusCode = WebSmsStatusCode.Ok,
                    StatusMessage = "OK",
                    TransferId = "tx"
                }, WebSmsJsonSerialization.DefaultOptions)));

        var request = new BinarySmsSendRequest
        {
            ClientMessageId = "id",
            RecipientAddressList = ["4367612345678"],
            MessageContent = ["AQID"]
        };

        // Act
        await _connector.SendBinaryMessage(request);

        // Assert
        var headers = SingleRequest().Headers.ShouldNotBeNull();
        headers["Authorization"].ToString().ShouldBe($"Bearer {BearerToken}");
    }

    [Test]
    public async Task SendTextMessage_WhenApiReturnsBadRequest_ThrowsHttpRequestException()
    {
        // Arrange
        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/text").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.BadRequest)
                .WithBody("invalid request"));

        var request = new TextSmsSendRequest
        {
            ClientMessageId = "id",
            RecipientAddressList = ["4367612345678"],
            MessageContent = "hello"
        };

        // Act / Assert
        var exception = await Should.ThrowAsync<HttpRequestException>(() => _connector.SendTextMessage(request));
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SendTextMessage_WhenApiReturnsInternalServerError_ThrowsHttpRequestException()
    {
        // Arrange
        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/text").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.InternalServerError));

        var request = new TextSmsSendRequest
        {
            ClientMessageId = "id",
            RecipientAddressList = ["4367612345678"],
            MessageContent = "hello"
        };

        // Act / Assert
        var exception = await Should.ThrowAsync<HttpRequestException>(() => _connector.SendTextMessage(request));
        exception.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }

    [Test]
    public async Task SendBinaryMessage_WhenApiReturnsBadRequest_ThrowsHttpRequestException()
    {
        // Arrange
        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/binary").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.BadRequest));

        var request = new BinarySmsSendRequest
        {
            ClientMessageId = "id",
            RecipientAddressList = ["4367612345678"],
            MessageContent = ["AQID"]
        };

        // Act / Assert
        var exception = await Should.ThrowAsync<HttpRequestException>(() => _connector.SendBinaryMessage(request));
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SendTextMessage_DeserializesNonOkBusinessStatusCode()
    {
        // Arrange — websms returns HTTP 200 even for some business errors (the API conveys them in `statusCode`).
        var businessErrorResponse = new MessageSendResponse
        {
            ClientMessageId = "id",
            SmsCount = 0,
            StatusCode = WebSmsStatusCode.InvalidRecipient,
            StatusMessage = "invalid recipient",
            TransferId = "tx"
        };

        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/text").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(businessErrorResponse, WebSmsJsonSerialization.DefaultOptions)));

        var request = new TextSmsSendRequest
        {
            ClientMessageId = "id",
            RecipientAddressList = ["invalid"],
            MessageContent = "hello"
        };

        // Act
        var response = await _connector.SendTextMessage(request);

        // Assert
        response.StatusCode.ShouldBe(WebSmsStatusCode.InvalidRecipient);
        response.StatusMessage.ShouldBe("invalid recipient");
    }
}
