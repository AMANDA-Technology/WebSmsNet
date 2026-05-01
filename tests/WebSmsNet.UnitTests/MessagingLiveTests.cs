using Shouldly;
using WebSmsNet.Abstractions;
using WebSmsNet.Abstractions.Configuration;
using WebSmsNet.Abstractions.Helpers;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.Abstractions.Models.Enums;

namespace WebSmsNet.UnitTests;

[TestFixture]
[Category("LiveTest")]
public class MessagingLiveTests
{
    private IWebSmsApiClient _webSmsApiClient = null!;

    [SetUp]
    public void SetUp()
    {
        var accessToken = Environment.GetEnvironmentVariable("Websms_AccessToken");
        Assume.That(accessToken, Is.Not.Null, "Websms_AccessToken env var not set — skipping live tests");

        _webSmsApiClient = new WebSmsApiClient(new WebSmsApiOptions
        {
            BaseUrl = "https://api.linkmobility.eu/",
            AuthenticationType = AuthenticationType.Bearer,
            AccessToken = accessToken!
        });
    }

    [Test]
    public async Task SendTextMessage()
    {
        // Arrange
        var recipientList = Environment.GetEnvironmentVariable("Websms_RecipientAddressList");
        Assume.That(recipientList, Is.Not.Null, "Websms_RecipientAddressList env var not set — skipping live tests");

        var request = new TextSmsSendRequest
        {
            ClientMessageId = Guid.NewGuid().ToString(),
            RecipientAddressList = [recipientList!],
            Test = true,
            MessageContent = "hi there! this is a test message that fits into 1 sms."
        };

        // Act
        var response = await _webSmsApiClient.Messaging.SendTextMessage(request);

        // Assert
        response.StatusCode.ShouldBeOneOf(WebSmsStatusCode.Ok, WebSmsStatusCode.OkQueued);
        response.SmsCount.ShouldBe(1);
        response.ClientMessageId.ShouldBe(request.ClientMessageId);
    }

    [Test]
    public async Task SendBinaryMessage()
    {
        // Arrange
        var recipientList = Environment.GetEnvironmentVariable("Websms_RecipientAddressList");
        Assume.That(recipientList, Is.Not.Null, "Websms_RecipientAddressList env var not set — skipping live tests");

        var request = new BinarySmsSendRequest
        {
            ClientMessageId = Guid.NewGuid().ToString(),
            RecipientAddressList = [recipientList!],
            Test = true,
            MessageContent = BinaryContent.CreateMessageContentParts("hi there! ", "this is a test message ", "with 3 sms.").ToList(),
            UserDataHeaderPresent = true
        };

        // Act
        var response = await _webSmsApiClient.Messaging.SendBinaryMessage(request);

        // Assert
        response.StatusCode.ShouldBeOneOf(WebSmsStatusCode.Ok, WebSmsStatusCode.OkQueued);
        response.SmsCount.ShouldBe(3);
        response.ClientMessageId.ShouldBe(request.ClientMessageId);
    }
}
