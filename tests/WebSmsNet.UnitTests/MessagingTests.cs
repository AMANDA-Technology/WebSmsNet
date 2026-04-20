using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WebSmsNet.Abstractions;
using WebSmsNet.Abstractions.Configuration;
using WebSmsNet.Abstractions.Helpers;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.Abstractions.Models.Enums;
using WebSmsNet.Abstractions.Serialization;
using WebSmsNet.AspNetCore.Configuration;
using WebSmsNet.AspNetCore.Helpers;

namespace WebSmsNet.UnitTests;

[TestFixture]
public class MessagingTests
{
    [Test]
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
        webSmsApiClient.ShouldNotBeNull();
    }

    [Test]
    public void ParseBinaryContent()
    {
        // Arrange
        var binaryContent = BinaryContent.CreateMessageContentParts("hi there! ", "this is a test message ", "with 3 sms.").ToList();

        // Act
        var text = BinaryContent.Parse(binaryContent, true);

        // Assert
        text.ShouldBe("hi there! this is a test message with 3 sms.");
    }

    [Test]
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
        responseJson.ShouldBe("""
                               {
                                 "clientMessageId": "5224d313-5c32-4024-aa04-61cc6bd2509d",
                                 "smsCount": 1,
                                 "statusCode": 2000,
                                 "statusMessage": "Test OK",
                                 "transferId": "6fd522e7126d4345"
                               }
                               """);
    }

    [Test]
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
        response.ShouldNotBeNull();
        response.ShouldBe(new MessageSendResponse
        {
            ClientMessageId = "5224d313-5c32-4024-aa04-61cc6bd2509d",
            SmsCount = 1,
            StatusCode = WebSmsStatusCode.Ok,
            StatusMessage = "Test OK",
            TransferId = "6fd522e7126d4345"
        });
    }

    [Test]
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
        request.ShouldBeOfType<WebSmsWebhookRequest.Text>();
        request.MessageType.ShouldBe(WebhookMessageType.Text);
        request.NotificationId.ShouldBe("02c1d0051949fe70cbfa");
    }

    [Test]
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
        var result = WebSmsWebhook.Parse(json).Match(
            onText: _ => true,
            onBinary: _ => false,
            onDeliveryReport: _ => false);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
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
        request.ShouldNotBeNull();
        request.ShouldBeOfType<WebSmsWebhookRequest.DeliveryReport>();
        request.MessageType.ShouldBe(WebhookMessageType.DeliveryReport);
        request.NotificationId.ShouldBe("5280675327899111111");
    }
}
