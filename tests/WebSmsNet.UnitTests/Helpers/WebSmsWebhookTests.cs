using System.Text;
using System.Text.Json;
using Shouldly;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.Abstractions.Models.Enums;
using WebSmsNet.AspNetCore.Helpers;

namespace WebSmsNet.UnitTests.Helpers;

[TestFixture]
public class WebSmsWebhookTests
{
    private const string TextWebhookJson = """
                                           {
                                               "messageType": "text",
                                               "notificationId": "02c1d0051949fe70cbfa",
                                               "senderAddress": "4367612345678",
                                               "senderAddressType": "international",
                                               "recipientAddress": "08282709900001",
                                               "recipientAddressType": "national",
                                               "textMessageContent": "Hello World"
                                           }
                                           """;

    private const string BinaryWebhookJson = """
                                             {
                                                 "messageType": "binary",
                                                 "notificationId": "xyz789",
                                                 "senderAddress": "4366012345678",
                                                 "senderAddressType": "international",
                                                 "recipientAddress": "066012345678",
                                                 "recipientAddressType": "national",
                                                 "userDataHeaderPresent": true,
                                                 "binaryMessageContent": ["AQID", "BAUG"]
                                             }
                                             """;

    private const string DeliveryReportWebhookJson = """
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

    [Test]
    public void Parse_String_TextJson_ReturnsTextRequest()
    {
        var result = WebSmsWebhook.Parse(TextWebhookJson);

        result.ShouldBeOfType<WebSmsWebhookRequest.Text>();
        result.MessageType.ShouldBe(WebhookMessageType.Text);
        result.NotificationId.ShouldBe("02c1d0051949fe70cbfa");
        ((WebSmsWebhookRequest.Text)result).TextMessageContent.ShouldBe("Hello World");
    }

    [Test]
    public void Parse_String_BinaryJson_ReturnsBinaryRequest()
    {
        var result = WebSmsWebhook.Parse(BinaryWebhookJson);

        result.ShouldBeOfType<WebSmsWebhookRequest.Binary>();
        result.MessageType.ShouldBe(WebhookMessageType.Binary);
        var binary = (WebSmsWebhookRequest.Binary)result;
        binary.UserDataHeaderPresent.ShouldBeTrue();
        binary.BinaryMessageContent.ShouldBe(["AQID", "BAUG"]);
    }

    [Test]
    public void Parse_String_DeliveryReportJson_ReturnsDeliveryReportRequest()
    {
        var result = WebSmsWebhook.Parse(DeliveryReportWebhookJson);

        result.ShouldBeOfType<WebSmsWebhookRequest.DeliveryReport>();
        result.MessageType.ShouldBe(WebhookMessageType.DeliveryReport);
        var report = (WebSmsWebhookRequest.DeliveryReport)result;
        report.TransferId.ShouldBe("00670eb55d00349e1111");
        report.DeliveryReportMessageStatus.ShouldBe(DeliveryReportMessageStatus.Delivered);
        report.ClientMessageId.ShouldBe("11cf996f-c59f-40db-bcff-c8ce03ce3a72");
    }

    [Test]
    public void Parse_String_InvalidJson_ThrowsJsonException()
    {
        Should.Throw<JsonException>(() => WebSmsWebhook.Parse("{not json"));
    }

    [Test]
    public void Parse_String_NullLiteral_ThrowsInvalidOperationException()
    {
        var ex = Should.Throw<InvalidOperationException>(() => WebSmsWebhook.Parse("null"));
        ex.Message.ShouldBe("Failed to deserialize web sms webhook request");
    }

    [Test]
    public async Task Parse_Stream_TextJson_ReturnsTextRequest()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TextWebhookJson));

        var result = await WebSmsWebhook.Parse(stream);

        result.ShouldBeOfType<WebSmsWebhookRequest.Text>();
        result.NotificationId.ShouldBe("02c1d0051949fe70cbfa");
        ((WebSmsWebhookRequest.Text)result).TextMessageContent.ShouldBe("Hello World");
    }

    [Test]
    public async Task Parse_Stream_RespectsCancellationToken()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TextWebhookJson));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            WebSmsWebhook.Parse(stream, cts.Token));
    }

    [Test]
    public void Match_Func_TextRequest_InvokesOnTextHandler()
    {
        WebSmsWebhookRequest.Base request = new WebSmsWebhookRequest.Text
        {
            MessageType = WebhookMessageType.Text,
            NotificationId = "test-id",
            SenderAddress = "4366012345678",
            SenderAddressType = AddressType.International,
            RecipientAddress = "066012345678",
            RecipientAddressType = AddressType.National,
            TextMessageContent = "Hello"
        };

        var result = request.Match(
            onText: _ => 1,
            onBinary: _ => 2,
            onDeliveryReport: _ => 3);

        result.ShouldBe(1);
    }

    [Test]
    public void Match_Func_BinaryRequest_InvokesOnBinaryHandler()
    {
        WebSmsWebhookRequest.Base request = new WebSmsWebhookRequest.Binary
        {
            MessageType = WebhookMessageType.Binary,
            NotificationId = "test-id",
            SenderAddress = "4366012345678",
            SenderAddressType = AddressType.International,
            RecipientAddress = "066012345678",
            RecipientAddressType = AddressType.National,
            UserDataHeaderPresent = false,
            BinaryMessageContent = ["AQID"]
        };

        var result = request.Match(
            onText: _ => 1,
            onBinary: _ => 2,
            onDeliveryReport: _ => 3);

        result.ShouldBe(2);
    }

    [Test]
    public void Match_Func_DeliveryReportRequest_InvokesOnDeliveryReportHandler()
    {
        WebSmsWebhookRequest.Base request = new WebSmsWebhookRequest.DeliveryReport
        {
            MessageType = WebhookMessageType.DeliveryReport,
            NotificationId = "test-id",
            SenderAddress = "4366012345678",
            TransferId = "transfer1",
            DeliveryReportMessageStatus = DeliveryReportMessageStatus.Delivered,
            SentOn = DateTimeOffset.Parse("2024-10-15T20:33:02.000+02:00"),
            ClientMessageId = "client1"
        };

        var result = request.Match(
            onText: _ => 1,
            onBinary: _ => 2,
            onDeliveryReport: _ => 3);

        result.ShouldBe(3);
    }

    [Test]
    public void Match_Func_UnknownSubtype_ThrowsArgumentOutOfRangeException()
    {
        WebSmsWebhookRequest.Base request = new UnknownWebhookRequest
        {
            MessageType = WebhookMessageType.Text,
            NotificationId = "test-id",
            SenderAddress = "4366012345678"
        };

        Should.Throw<ArgumentOutOfRangeException>(() => request.Match(
            onText: _ => 1,
            onBinary: _ => 2,
            onDeliveryReport: _ => 3));
    }

    [Test]
    public void Match_Action_TextRequest_InvokesOnTextHandler()
    {
        WebSmsWebhookRequest.Base request = new WebSmsWebhookRequest.Text
        {
            MessageType = WebhookMessageType.Text,
            NotificationId = "test-id",
            SenderAddress = "4366012345678",
            SenderAddressType = AddressType.International,
            RecipientAddress = "066012345678",
            RecipientAddressType = AddressType.National,
            TextMessageContent = "Hello"
        };
        var textCount = 0;
        var binaryCount = 0;
        var deliveryReportCount = 0;

        request.Match(
            onText: _ => textCount++,
            onBinary: _ => binaryCount++,
            onDeliveryReport: _ => deliveryReportCount++);

        textCount.ShouldBe(1);
        binaryCount.ShouldBe(0);
        deliveryReportCount.ShouldBe(0);
    }

    [Test]
    public void Match_Action_BinaryRequest_InvokesOnBinaryHandler()
    {
        WebSmsWebhookRequest.Base request = new WebSmsWebhookRequest.Binary
        {
            MessageType = WebhookMessageType.Binary,
            NotificationId = "test-id",
            SenderAddress = "4366012345678",
            SenderAddressType = AddressType.International,
            RecipientAddress = "066012345678",
            RecipientAddressType = AddressType.National,
            UserDataHeaderPresent = false,
            BinaryMessageContent = ["AQID"]
        };
        var textCount = 0;
        var binaryCount = 0;
        var deliveryReportCount = 0;

        request.Match(
            onText: _ => textCount++,
            onBinary: _ => binaryCount++,
            onDeliveryReport: _ => deliveryReportCount++);

        textCount.ShouldBe(0);
        binaryCount.ShouldBe(1);
        deliveryReportCount.ShouldBe(0);
    }

    [Test]
    public void Match_Action_DeliveryReportRequest_InvokesOnDeliveryReportHandler()
    {
        WebSmsWebhookRequest.Base request = new WebSmsWebhookRequest.DeliveryReport
        {
            MessageType = WebhookMessageType.DeliveryReport,
            NotificationId = "test-id",
            SenderAddress = "4366012345678",
            TransferId = "transfer1",
            DeliveryReportMessageStatus = DeliveryReportMessageStatus.Delivered,
            SentOn = DateTimeOffset.Parse("2024-10-15T20:33:02.000+02:00"),
            ClientMessageId = "client1"
        };
        var textCount = 0;
        var binaryCount = 0;
        var deliveryReportCount = 0;

        request.Match(
            onText: _ => textCount++,
            onBinary: _ => binaryCount++,
            onDeliveryReport: _ => deliveryReportCount++);

        textCount.ShouldBe(0);
        binaryCount.ShouldBe(0);
        deliveryReportCount.ShouldBe(1);
    }

    [Test]
    public void Match_Action_UnknownSubtype_ThrowsArgumentOutOfRangeException()
    {
        WebSmsWebhookRequest.Base request = new UnknownWebhookRequest
        {
            MessageType = WebhookMessageType.Text,
            NotificationId = "test-id",
            SenderAddress = "4366012345678"
        };

        Should.Throw<ArgumentOutOfRangeException>(() => request.Match(
            onText: _ => { },
            onBinary: _ => { },
            onDeliveryReport: _ => { }));
    }

    [Test]
    public void CreateOkResponse_ReturnsResponseWithStatusCodeOkAndOkMessage()
    {
        var response = WebSmsWebhook.CreateOkResponse();

        response.StatusCode.ShouldBe(WebSmsStatusCode.Ok);
        response.StatusMessage.ShouldBe("OK");
    }

    [Test]
    public void CreateErrorResponse_ReturnsResponseWithInternalErrorAndProvidedMessage()
    {
        var response = WebSmsWebhook.CreateErrorResponse("boom");

        response.StatusCode.ShouldBe(WebSmsStatusCode.InternalError);
        response.StatusMessage.ShouldBe("boom");
    }

    [Test]
    public void CreateErrorResponse_NullMessage_PropagatesNull()
    {
        var response = WebSmsWebhook.CreateErrorResponse(null!);

        response.StatusCode.ShouldBe(WebSmsStatusCode.InternalError);
        response.StatusMessage.ShouldBeNull();
    }

    private sealed class UnknownWebhookRequest : WebSmsWebhookRequest.Base;
}
