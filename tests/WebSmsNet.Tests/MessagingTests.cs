using FluentAssertions;
using WebSmsNet.Abstractions;
using WebSmsNet.Abstractions.Configuration;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.Abstractions.Models.Enums;
using WebSmsNet.AspNetCore.Configuration;
using WebSmsNet.AspNetCore.Helpers;

namespace WebSmsNet.Tests;

public class MessagingTests
{
    private readonly IWebSmsApiClient _webSmsApiClient = new WebSmsApiClient(new(new HttpClient().ApplyWebSmsApiOptions(new()
    {
        BaseUrl = "https://api.linkmobility.eu/",
        AuthenticationType = AuthenticationType.Bearer,
        AccessToken = Environment.GetEnvironmentVariable("Websms_AccessToken") ?? throw new InvalidOperationException("Missing AccessToken")
    })));

    [Fact]
    public async Task Test1_SendTextMessageAsync()
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
        var response = await _webSmsApiClient.Messaging.SendTextMessageAsync(request);

        // Assert
        Assert.NotNull(response);
        response.StatusCode.Should().BeOneOf(WebSmsStatusCode.Ok, WebSmsStatusCode.OkQueued);
        response.SmsCount.Should().Be(1);
        response.ClientMessageId.Should().Be(request.ClientMessageId);
    }

    [Fact]
    public async Task Test2_SendBinaryMessageAsync()
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
        var response = await _webSmsApiClient.Messaging.SendBinaryMessageAsync(request);

        // Assert
        Assert.NotNull(response);
        response.StatusCode.Should().BeOneOf(WebSmsStatusCode.Ok, WebSmsStatusCode.OkQueued);
        response.SmsCount.Should().Be(1);
        response.ClientMessageId.Should().Be(request.ClientMessageId);
    }

    [Fact]
    public void Test3_ParseWebhookRequest()
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
        Assert.NotNull(request);
        request.Should().BeOfType<WebSmsWebhookRequest.Text>();
        request.MessageType.Should().Be(WebhookMessageType.Text);
        request.NotificationId.Should().Be("02c1d0051949fe70cbfa");
    }
}
