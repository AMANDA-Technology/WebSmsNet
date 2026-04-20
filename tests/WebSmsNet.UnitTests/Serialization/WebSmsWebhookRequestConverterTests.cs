using System.Text.Json;
using Shouldly;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.Abstractions.Models.Enums;
using WebSmsNet.Abstractions.Serialization;

namespace WebSmsNet.UnitTests.Serialization;

[TestFixture]
public class WebSmsWebhookRequestConverterTests
{
    private JsonSerializerOptions _options = null!;

    [SetUp]
    public void SetUp()
    {
        _options = WebSmsJsonSerialization.DefaultOptions;
    }

    [Test]
    public void Read_TextMessage_ReturnsTextType()
    {
        const string json = """
                            {
                                "messageType": "text",
                                "notificationId": "abc123",
                                "senderAddress": "4367612345678",
                                "senderAddressType": "international",
                                "recipientAddress": "08282709900001",
                                "recipientAddressType": "national",
                                "textMessageContent": "Hello World"
                            }
                            """;

        var result = JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<WebSmsWebhookRequest.Text>();
        result.MessageType.ShouldBe(WebhookMessageType.Text);
        result.NotificationId.ShouldBe("abc123");
        result.SenderAddress.ShouldBe("4367612345678");
        ((WebSmsWebhookRequest.Text)result).TextMessageContent.ShouldBe("Hello World");
    }

    [Test]
    public void Read_BinaryMessage_ReturnsBinaryType()
    {
        const string json = """
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

        var result = JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<WebSmsWebhookRequest.Binary>();
        result.MessageType.ShouldBe(WebhookMessageType.Binary);
        result.NotificationId.ShouldBe("xyz789");
        var binary = (WebSmsWebhookRequest.Binary)result;
        binary.UserDataHeaderPresent.ShouldBeTrue();
        binary.BinaryMessageContent.ShouldBe(["AQID", "BAUG"]);
    }

    [Test]
    public void Read_DeliveryReport_ReturnsDeliveryReportType()
    {
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

        var result = JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<WebSmsWebhookRequest.DeliveryReport>();
        result.MessageType.ShouldBe(WebhookMessageType.DeliveryReport);
        var report = (WebSmsWebhookRequest.DeliveryReport)result;
        report.TransferId.ShouldBe("00670eb55d00349e1111");
        report.DeliveryReportMessageStatus.ShouldBe(DeliveryReportMessageStatus.Delivered);
        report.ClientMessageId.ShouldBe("11cf996f-c59f-40db-bcff-c8ce03ce3a72");
    }

    [Test]
    public void Read_MissingMessageType_ThrowsJsonException()
    {
        const string json = """
                            {
                                "notificationId": "abc123",
                                "senderAddress": "4367612345678"
                            }
                            """;

        Should.Throw<JsonException>(() =>
            JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options));
    }

    [Test]
    public void Read_UnknownMessageType_ThrowsJsonException()
    {
        const string json = """
                            {
                                "messageType": "voice",
                                "notificationId": "abc123",
                                "senderAddress": "4367612345678"
                            }
                            """;

        Should.Throw<JsonException>(() =>
            JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options));
    }

    [Test]
    public void Write_TextMessage_SerializesWithMessageType()
    {
        var text = new WebSmsWebhookRequest.Text
        {
            MessageType = WebhookMessageType.Text,
            NotificationId = "abc123",
            SenderAddress = "4367612345678",
            SenderAddressType = AddressType.International,
            RecipientAddress = "08282709900001",
            RecipientAddressType = AddressType.National,
            TextMessageContent = "Hello"
        };

        var json = JsonSerializer.Serialize<WebSmsWebhookRequest.Base>(text, _options);

        json.ShouldContain("\"messageType\":\"text\"");
        json.ShouldContain("\"notificationId\":\"abc123\"");
        json.ShouldContain("\"textMessageContent\":\"Hello\"");
    }

    [Test]
    public void Write_DeliveryReport_SerializesWithMessageType()
    {
        var report = new WebSmsWebhookRequest.DeliveryReport
        {
            MessageType = WebhookMessageType.DeliveryReport,
            NotificationId = "notif1",
            SenderAddress = "41791111111",
            TransferId = "transfer1",
            DeliveryReportMessageStatus = DeliveryReportMessageStatus.Delivered,
            SentOn = DateTimeOffset.Parse("2024-10-15T20:33:02.000+02:00"),
            ClientMessageId = "client1"
        };

        var json = JsonSerializer.Serialize<WebSmsWebhookRequest.Base>(report, _options);

        json.ShouldContain("\"messageType\":\"deliveryReport\"");
        json.ShouldContain("\"transferId\":\"transfer1\"");
        json.ShouldContain("\"deliveryReportMessageStatus\":\"delivered\"");
    }

    [Test]
    public void RoundTrip_TextMessage_PreservesAllFields()
    {
        const string json = """
                            {
                                "messageType": "text",
                                "notificationId": "roundtrip1",
                                "senderAddress": "4367612345678",
                                "senderAddressType": "international",
                                "recipientAddress": "08282709900001",
                                "recipientAddressType": "national",
                                "textMessageContent": "Round-trip test"
                            }
                            """;

        var deserialized = JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options);
        deserialized.ShouldNotBeNull();

        var reserialized = JsonSerializer.Serialize(deserialized, deserialized.GetType(), _options);
        var back = JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(reserialized, _options);

        back.ShouldNotBeNull();
        back.ShouldBeOfType<WebSmsWebhookRequest.Text>();
        back.NotificationId.ShouldBe("roundtrip1");
        ((WebSmsWebhookRequest.Text)back).TextMessageContent.ShouldBe("Round-trip test");
    }
}
