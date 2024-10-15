using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WebSmsNet.Abstractions;
using WebSmsNet.Abstractions.Configuration;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.Abstractions.Models.Enums;
using WebSmsNet.Abstractions.Serialization;
using WebSmsNet.AspNetCore.Configuration;
using WebSmsNet.AspNetCore.Helpers;

namespace WebSmsNet.Tests;

public class MessagingTests
{
    private readonly IWebSmsApiClient _webSmsApiClient = new WebSmsApiClient(new WebSmsApiOptions
    {
        BaseUrl = "https://api.linkmobility.eu/",
        AuthenticationType = AuthenticationType.Bearer,
        AccessToken = Environment.GetEnvironmentVariable("Websms_AccessToken") ?? throw new InvalidOperationException("Missing AccessToken")
    });

    [Fact]
    public void DependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWebSmsApiClient(options =>
        {
            options.BaseUrl = "https://api.linkmobility.eu/";
            options.AuthenticationType = AuthenticationType.Bearer;
            options.AccessToken = "YOUR ACCESS_TOKEN";
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var webSmsApiClient = provider.GetService<IWebSmsApiClient>();
        Assert.NotNull(webSmsApiClient);
    }

    [Fact]
    public async Task SendTextMessage()
    {
        // Arrange
        var request = new TextSmsSendRequest
        {
            ClientMessageId = Guid.NewGuid().ToString(),
            RecipientAddressList =
            [
                Environment.GetEnvironmentVariable("Websms_RecipientAddressList") ?? throw new InvalidOperationException("Missing RecipientAddressList")
            ],
            Test = true,
            MessageContent = "hi there! this is a test message."
        };

        // Act
        var response = await _webSmsApiClient.Messaging.SendTextMessage(request);

        // Assert
        response.StatusCode.Should().BeOneOf(WebSmsStatusCode.Ok, WebSmsStatusCode.OkQueued);
        response.SmsCount.Should().Be(1);
        response.ClientMessageId.Should().Be(request.ClientMessageId);
    }

    [Fact]
    public async Task SendBinaryMessage()
    {
        // Arrange
        var request = new BinarySmsSendRequest
        {
            ClientMessageId = Guid.NewGuid().ToString(),
            RecipientAddressList =
            [
                Environment.GetEnvironmentVariable("Websms_RecipientAddressList") ?? throw new InvalidOperationException("Missing RecipientAddressList")
            ],
            Test = true,
            MessageContent = [Convert.ToBase64String("hi there! this is a test message."u8.ToArray())]
        };

        // Act
        var response = await _webSmsApiClient.Messaging.SendBinaryMessage(request);

        // Assert
        response.StatusCode.Should().BeOneOf(WebSmsStatusCode.Ok, WebSmsStatusCode.OkQueued);
        response.SmsCount.Should().Be(1);
        response.ClientMessageId.Should().Be(request.ClientMessageId);
    }

    [Fact]
    public void MessageSendResponse_Serialize()
    {
        // Arrange
        var response = new MessageSendResponse
        {
            ClientMessageId = "5224d313-5c32-4024-aa04-61cc6bd2509d",
            SmsCount = 1,
            StatusCode = WebSmsStatusCode.Ok,
            StatusMessage = "Test OK",
            TransferId = "6fd522e7126d4345"
        };

        var options = WebSmsJsonSerialization.DefaultOptions;
        options.WriteIndented = true;

        // Act
        var responseJson = JsonSerializer.Serialize(response, options);

        // Assert
        responseJson.Should().BeEquivalentTo("""
                                             {
                                               "clientMessageId": "5224d313-5c32-4024-aa04-61cc6bd2509d",
                                               "smsCount": 1,
                                               "statusCode": 2000,
                                               "statusMessage": "Test OK",
                                               "transferId": "6fd522e7126d4345"
                                             }
                                             """);
    }

    [Fact]
    public void MessageSendResponse_Deserialize()
    {
        // Arrange
        const string responseJson = """
                                    {
                                      "clientMessageId": "5224d313-5c32-4024-aa04-61cc6bd2509d",
                                      "smsCount": 1,
                                      "statusCode": 2000,
                                      "statusMessage": "Test OK",
                                      "transferId": "6fd522e7126d4345"
                                    }
                                    """;

        // Act
        var response = JsonSerializer.Deserialize<MessageSendResponse>(responseJson, WebSmsJsonSerialization.DefaultOptions);

        // Assert
        Assert.NotNull(response);
        response.Should().Be(new MessageSendResponse
        {
            ClientMessageId = "5224d313-5c32-4024-aa04-61cc6bd2509d",
            SmsCount = 1,
            StatusCode = WebSmsStatusCode.Ok,
            StatusMessage = "Test OK",
            TransferId = "6fd522e7126d4345"
        });
    }

    [Fact]
    public void WebhookRequest_Parse_Text()
    {
        // Arrange
        const string json = """
                            {
                                "messageType": "text",
                                "notificationId": "02c1d0051949fe70cbfa",
                                "senderAddress": "4367612345678",
                                "senderAddressType": "international",
                                "recipientAddress": "08282709900001",
                                "recipientAddressType": "national",
                                "textMessageContent": "Das ist eine Antwort SMS mit Sonderzeichen, Umlauten \u003c\"Ümläuten\"\u003e und €urozeichen."
                            }
                            """;

        // Act
        var request = WebSmsWebhook.Parse(json);

        // Assert
        request.Should().BeOfType<WebSmsWebhookRequest.Text>();
        request.MessageType.Should().Be(WebhookMessageType.Text);
        request.NotificationId.Should().Be("02c1d0051949fe70cbfa");
    }

    [Fact]
    public void WebhookRequest_Match_Text()
    {
        // Arrange
        const string json = """
                            {
                                "messageType": "text",
                                "notificationId": "02c1d0051949fe70cbfa",
                                "senderAddress": "4367612345678",
                                "senderAddressType": "international",
                                "recipientAddress": "08282709900001",
                                "recipientAddressType": "national",
                                "textMessageContent": "Das ist eine Antwort SMS mit Sonderzeichen, Umlauten \u003c\"Ümläuten\"\u003e und €urozeichen."
                            }
                            """;

        // Act
        var request = WebSmsWebhook.Parse(json).Match(
            onText: _ => true,
            onBinary: _ => false,
            onDeliveryReport: _ => false);

        // Assert
        request.Should().BeTrue();
    }

    [Fact]
    public void WebhookRequest_Parse_DeliveryReport()
    {
        // Arrange
        const string json = """
                            {
                                "messageType": "deliveryReport",
                                "notificationId": "5280675327899111111",
                                "transferId": "00670eb55d00349e1111",
                                "senderAddress": "41791111111",
                                "deliveryReportMessageStatus": "delivered",
                                "sentOn": "2024-10-15T20:33:02.000+02:00",
                                "deliveredOn": "2024-10-15T20:33:03.000+02:00",
                                "clientMessageId": "11cf996f-c59f-40db-bcff-c8ce03ce3a72"
                            }
                            """;

        // Act
        var request = WebSmsWebhook.Parse(json);

        // Assert
        Assert.NotNull(request);
        request.Should().BeOfType<WebSmsWebhookRequest.DeliveryReport>();
        request.MessageType.Should().Be(WebhookMessageType.DeliveryReport);
        request.NotificationId.Should().Be("5280675327899111111");
    }
}
