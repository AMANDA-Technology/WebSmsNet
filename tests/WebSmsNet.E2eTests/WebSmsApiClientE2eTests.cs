using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WebSmsNet.Abstractions;
using WebSmsNet.Abstractions.Configuration;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.Abstractions.Models.Enums;
using WebSmsNet.Abstractions.Serialization;
using WebSmsNet.AspNetCore.Configuration;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace WebSmsNet.E2eTests;

[TestFixture]
public class WebSmsApiClientE2eTests
{
    private const string BearerToken = "test-token";
    private const string BasicUsername = "test-user";
    private const string BasicPassword = "test-pass";

    private WireMockServer _server = null!;

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
    }

    private ServiceProvider BuildBearerProvider() =>
        new ServiceCollection()
            .AddWebSmsApiClient(options =>
            {
                options.BaseUrl = _server.Url!;
                options.AuthenticationType = AuthenticationType.Bearer;
                options.AccessToken = BearerToken;
            })
            .BuildServiceProvider();

    private ServiceProvider BuildBasicProvider() =>
        new ServiceCollection()
            .AddWebSmsApiClient(options =>
            {
                options.BaseUrl = _server.Url!;
                options.AuthenticationType = AuthenticationType.Basic;
                options.Username = BasicUsername;
                options.Password = BasicPassword;
            })
            .BuildServiceProvider();

    [Test]
    public async Task Messaging_SendTextSms_WithBearerAuth_ConstructsFullPipelineAndReturnsResponse()
    {
        // Arrange
        var expectedResponse = new MessageSendResponse
        {
            ClientMessageId = "e2e-text-1",
            SmsCount = 1,
            StatusCode = WebSmsStatusCode.Ok,
            StatusMessage = "OK",
            TransferId = "tx-e2e-text-1"
        };

        _server
            .Given(Request.Create()
                .WithPath("/rest/smsmessaging/text")
                .UsingPost()
                .WithHeader("Authorization", $"Bearer {BearerToken}")
                .WithHeader("Accept", "application/json"))
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(expectedResponse, WebSmsJsonSerialization.DefaultOptions)));

        await using var provider = BuildBearerProvider();
        using var scope = provider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IWebSmsApiClient>();

        var request = new TextSmsSendRequest
        {
            ClientMessageId = "e2e-text-1",
            RecipientAddressList = ["4367612345678"],
            MessageContent = "hello e2e"
        };

        // Act
        var response = await client.Messaging.SendTextMessage(request);

        // Assert
        response.ShouldBe(expectedResponse);
    }

    [Test]
    public async Task Messaging_SendTextSms_WithBasicAuth_ConstructsFullPipelineAndReturnsResponse()
    {
        // Arrange
        var expectedBasicHeader =
            "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{BasicUsername}:{BasicPassword}"));

        var expectedResponse = new MessageSendResponse
        {
            ClientMessageId = "e2e-basic-1",
            SmsCount = 1,
            StatusCode = WebSmsStatusCode.Ok,
            StatusMessage = "OK",
            TransferId = "tx-e2e-basic-1"
        };

        _server
            .Given(Request.Create()
                .WithPath("/rest/smsmessaging/text")
                .UsingPost()
                .WithHeader("Authorization", expectedBasicHeader))
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(expectedResponse, WebSmsJsonSerialization.DefaultOptions)));

        await using var provider = BuildBasicProvider();
        using var scope = provider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IWebSmsApiClient>();

        var request = new TextSmsSendRequest
        {
            ClientMessageId = "e2e-basic-1",
            RecipientAddressList = ["4367612345678"],
            MessageContent = "hello basic"
        };

        // Act
        var response = await client.Messaging.SendTextMessage(request);

        // Assert
        response.ShouldBe(expectedResponse);
    }

    [Test]
    public async Task Messaging_SendBinarySms_WithBearerAuth_ConstructsFullPipelineAndReturnsResponse()
    {
        // Arrange
        var expectedResponse = new MessageSendResponse
        {
            ClientMessageId = "e2e-binary-1",
            SmsCount = 2,
            StatusCode = WebSmsStatusCode.OkQueued,
            StatusMessage = "Queued",
            TransferId = "tx-e2e-binary-1"
        };

        _server
            .Given(Request.Create()
                .WithPath("/rest/smsmessaging/binary")
                .UsingPost()
                .WithHeader("Authorization", $"Bearer {BearerToken}"))
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(expectedResponse, WebSmsJsonSerialization.DefaultOptions)));

        await using var provider = BuildBearerProvider();
        using var scope = provider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IWebSmsApiClient>();

        var request = new BinarySmsSendRequest
        {
            ClientMessageId = "e2e-binary-1",
            RecipientAddressList = ["4367612345678"],
            MessageContent = ["AQID", "BAUG"],
            UserDataHeaderPresent = true
        };

        // Act
        var response = await client.Messaging.SendBinaryMessage(request);

        // Assert
        response.ShouldBe(expectedResponse);
    }

    [Test]
    public async Task Messaging_RegisteredViaDi_ResolvesAndDispatchesRequestToCorrectEndpoint()
    {
        // Arrange
        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/text").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new MessageSendResponse
                {
                    ClientMessageId = "e2e-routing",
                    SmsCount = 1,
                    StatusCode = WebSmsStatusCode.Ok,
                    StatusMessage = "OK",
                    TransferId = "tx"
                }, WebSmsJsonSerialization.DefaultOptions)));

        await using var provider = BuildBearerProvider();
        using var scope = provider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IWebSmsApiClient>();

        // Act
        await client.Messaging.SendTextMessage(new TextSmsSendRequest
        {
            ClientMessageId = "e2e-routing",
            RecipientAddressList = ["4367612345678"],
            MessageContent = "routing test"
        });

        // Assert — verify the request actually reached WireMock on the expected path.
        var logEntry = _server.LogEntries.Single();
        var requestMessage = logEntry.RequestMessage.ShouldNotBeNull();
        requestMessage.AbsolutePath.ShouldBe("/rest/smsmessaging/text");
        requestMessage.Method.ShouldBe("POST");
    }

    [Test]
    public async Task Messaging_SendTextSms_WithAllOptionalFields_SendsExactlyTheDocumentedJsonFields()
    {
        // Arrange
        var expectedResponse = new MessageSendResponse
        {
            ClientMessageId = "e2e-full",
            SmsCount = 1,
            StatusCode = WebSmsStatusCode.Ok,
            StatusMessage = "OK",
            TransferId = "tx-e2e-full"
        };

        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/text").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(expectedResponse, WebSmsJsonSerialization.DefaultOptions)));

        await using var provider = BuildBearerProvider();
        using var scope = provider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IWebSmsApiClient>();

        var request = new TextSmsSendRequest
        {
            ClientMessageId = "e2e-full",
            ContentCategory = ContentCategory.Informational,
            NotificationCallbackUrl = "https://example.test/cb",
            Priority = 1,
            RecipientAddressList = ["4367612345678"],
            SendAsFlashSms = true,
            SenderAddress = "AmandaTech",
            SenderAddressType = AddressType.Alphanumeric,
            Test = true,
            ValidityPeriod = 60,
            MaxSmsPerMessage = 1,
            MessageContent = "all fields populated",
            MessageType = MessageType.Default
        };

        // Act
        var response = await client.Messaging.SendTextMessage(request);

        // Assert
        response.ShouldBe(expectedResponse);

        var logEntry = _server.LogEntries.Single();
        var body = logEntry.RequestMessage.ShouldNotBeNull().Body.ShouldNotBeNull();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.GetProperty("clientMessageId").GetString().ShouldBe("e2e-full");
        root.GetProperty("contentCategory").GetString().ShouldBe("informational");
        root.GetProperty("notificationCallbackUrl").GetString().ShouldBe("https://example.test/cb");
        root.GetProperty("priority").GetInt32().ShouldBe(1);
        root.GetProperty("recipientAddressList").EnumerateArray().Single().GetString().ShouldBe("4367612345678");
        root.GetProperty("sendAsFlashSms").GetBoolean().ShouldBeTrue();
        root.GetProperty("senderAddress").GetString().ShouldBe("AmandaTech");
        root.GetProperty("senderAddressType").GetString().ShouldBe("alphanumeric");
        root.GetProperty("test").GetBoolean().ShouldBeTrue();
        root.GetProperty("validityPeriode").GetInt32().ShouldBe(60);
        root.GetProperty("maxSmsPerMessage").GetInt32().ShouldBe(1);
        root.GetProperty("messageContent").GetString().ShouldBe("all fields populated");
        root.GetProperty("messageType").GetString().ShouldBe("default");
    }

    [Test]
    public async Task Messaging_SendTextSms_WhenApiOmitsClientMessageId_DeserializesResponseSuccessfully()
    {
        // Arrange — clientMessageId is optional in the request, and the API only echoes it back when supplied.
        _server
            .Given(Request.Create().WithPath("/rest/smsmessaging/text").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                          {
                            "smsCount": 1,
                            "statusCode": 2000,
                            "statusMessage": "OK",
                            "transferId": "tx-e2e-no-cmid"
                          }
                          """));

        await using var provider = BuildBearerProvider();
        using var scope = provider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IWebSmsApiClient>();

        var request = new TextSmsSendRequest
        {
            RecipientAddressList = ["4367612345678"],
            MessageContent = "no client message id"
        };

        // Act
        var response = await client.Messaging.SendTextMessage(request);

        // Assert
        response.ClientMessageId.ShouldBeNull();
        response.SmsCount.ShouldBe(1);
        response.StatusCode.ShouldBe(WebSmsStatusCode.Ok);
        response.StatusMessage.ShouldBe("OK");
        response.TransferId.ShouldBe("tx-e2e-no-cmid");
    }
}
