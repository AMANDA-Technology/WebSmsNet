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
    public void Read_TextMessage_WithFlashSms_PreservesFlag()
    {
        const string json = """
                            {
                                "messageType": "text",
                                "notificationId": "abc123",
                                "senderAddress": "4367612345678",
                                "senderAddressType": "international",
                                "recipientAddress": "08282709900001",
                                "recipientAddressType": "national",
                                "messageFlashSms": true,
                                "textMessageContent": "Flash"
                            }
                            """;

        var text = (WebSmsWebhookRequest.Text)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options)!;

        text.MessageFlashSms.ShouldBeTrue();
    }

    [Test]
    public void Read_TextMessage_WithoutFlashSms_DefaultsToFalse()
    {
        const string json = """
                            {
                                "messageType": "text",
                                "notificationId": "abc123",
                                "senderAddress": "4367612345678",
                                "senderAddressType": "international",
                                "recipientAddress": "08282709900001",
                                "recipientAddressType": "national",
                                "textMessageContent": "Plain"
                            }
                            """;

        var text = (WebSmsWebhookRequest.Text)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options)!;

        text.MessageFlashSms.ShouldBeFalse();
    }

    [Test]
    public void Read_TextMessage_WithUtf8Payload_PreservesContent()
    {
        const string content = "Antwort SMS mit Sonderzeichen, Umlauten <\\\"Ümläuten\\\"> und €urozeichen.";
        var json = $$"""
                     {
                         "messageType": "text",
                         "notificationId": "02c1d0051949fe70cbfa",
                         "senderAddress": "4367612345678",
                         "senderAddressType": "international",
                         "recipientAddress": "08282709900001",
                         "recipientAddressType": "national",
                         "textMessageContent": "{{content}}"
                     }
                     """;

        var text = (WebSmsWebhookRequest.Text)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options)!;

        text.TextMessageContent.ShouldBe("Antwort SMS mit Sonderzeichen, Umlauten <\"Ümläuten\"> und €urozeichen.");
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
    public void Read_BinaryMessage_WithoutUserDataHeaderPresent_DefaultsToFalse()
    {
        const string json = """
                            {
                                "messageType": "binary",
                                "notificationId": "xyz789",
                                "senderAddress": "4366012345678",
                                "senderAddressType": "international",
                                "recipientAddress": "066012345678",
                                "recipientAddressType": "national",
                                "binaryMessageContent": ["AQID"]
                            }
                            """;

        var binary = (WebSmsWebhookRequest.Binary)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options)!;

        binary.UserDataHeaderPresent.ShouldBeFalse();
        binary.BinaryMessageContent.ShouldBe(["AQID"]);
    }

    [Test]
    public void Read_BinaryMessage_WithEmptyContentArray_PreservesEmptyArray()
    {
        const string json = """
                            {
                                "messageType": "binary",
                                "notificationId": "xyz789",
                                "senderAddress": "4366012345678",
                                "senderAddressType": "international",
                                "recipientAddress": "066012345678",
                                "recipientAddressType": "national",
                                "userDataHeaderPresent": false,
                                "binaryMessageContent": []
                            }
                            """;

        var binary = (WebSmsWebhookRequest.Binary)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options)!;

        binary.BinaryMessageContent.ShouldBeEmpty();
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
                                "deliveredAs": "sms",
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
        report.DeliveredOn.ShouldNotBeNull();
        report.DeliveredAs.ShouldBe(DeliveredAs.Sms);
        report.ClientMessageId.ShouldBe("11cf996f-c59f-40db-bcff-c8ce03ce3a72");
    }

    [Test]
    public void Read_DeliveryReport_WithoutOptionalFields_LeavesThemNull()
    {
        const string json = """
                            {
                                "messageType": "deliveryReport",
                                "notificationId": "5280675327899111111",
                                "transferId": "00670eb55d00349e1111",
                                "senderAddress": "41791111111",
                                "deliveryReportMessageStatus": "undelivered",
                                "sentOn": "2024-10-15T20:33:02.000+02:00"
                            }
                            """;

        var report = (WebSmsWebhookRequest.DeliveryReport)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options)!;

        report.DeliveryReportMessageStatus.ShouldBe(DeliveryReportMessageStatus.Undelivered);
        report.DeliveredOn.ShouldBeNull();
        report.DeliveredAs.ShouldBeNull();
        report.ClientMessageId.ShouldBeNull();
    }

    [Test]
    public void Read_DeliveryReport_WithExplicitNullOptionalFields_LeavesThemNull()
    {
        const string json = """
                            {
                                "messageType": "deliveryReport",
                                "notificationId": "5280675327899111111",
                                "transferId": "00670eb55d00349e1111",
                                "senderAddress": "41791111111",
                                "deliveryReportMessageStatus": "expired",
                                "sentOn": "2024-10-15T20:33:02.000+02:00",
                                "deliveredOn": null,
                                "deliveredAs": null,
                                "clientMessageId": null
                            }
                            """;

        var report = (WebSmsWebhookRequest.DeliveryReport)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options)!;

        report.DeliveryReportMessageStatus.ShouldBe(DeliveryReportMessageStatus.Expired);
        report.DeliveredOn.ShouldBeNull();
        report.DeliveredAs.ShouldBeNull();
        report.ClientMessageId.ShouldBeNull();
    }

    [Test]
    public void Read_DeliveryReport_FailoverSms_DeserializesHyphenatedValue()
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
                                "deliveredAs": "failover-sms"
                            }
                            """;

        var report = (WebSmsWebhookRequest.DeliveryReport)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options)!;

        report.DeliveredAs.ShouldBe(DeliveredAs.FailoverSms);
    }

    [TestCase("delivered", DeliveryReportMessageStatus.Delivered)]
    [TestCase("undelivered", DeliveryReportMessageStatus.Undelivered)]
    [TestCase("expired", DeliveryReportMessageStatus.Expired)]
    [TestCase("deleted", DeliveryReportMessageStatus.Deleted)]
    [TestCase("accepted", DeliveryReportMessageStatus.Accepted)]
    [TestCase("rejected", DeliveryReportMessageStatus.Rejected)]
    public void Read_DeliveryReport_AllStatusValues_AreParsed(string wireValue, DeliveryReportMessageStatus expected)
    {
        var json = $$"""
                     {
                         "messageType": "deliveryReport",
                         "notificationId": "n",
                         "transferId": "t",
                         "senderAddress": "41791111111",
                         "deliveryReportMessageStatus": "{{wireValue}}",
                         "sentOn": "2024-10-15T20:33:02.000+02:00"
                     }
                     """;

        var report = (WebSmsWebhookRequest.DeliveryReport)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options)!;

        report.DeliveryReportMessageStatus.ShouldBe(expected);
    }

    [TestCase("sms", DeliveredAs.Sms)]
    [TestCase("push", DeliveredAs.Push)]
    [TestCase("failover-sms", DeliveredAs.FailoverSms)]
    [TestCase("voice", DeliveredAs.Voice)]
    public void Read_DeliveryReport_AllDeliveredAsValues_AreParsed(string wireValue, DeliveredAs expected)
    {
        var json = $$"""
                     {
                         "messageType": "deliveryReport",
                         "notificationId": "n",
                         "transferId": "t",
                         "senderAddress": "41791111111",
                         "deliveryReportMessageStatus": "delivered",
                         "sentOn": "2024-10-15T20:33:02.000+02:00",
                         "deliveredOn": "2024-10-15T20:33:03.000+02:00",
                         "deliveredAs": "{{wireValue}}"
                     }
                     """;

        var report = (WebSmsWebhookRequest.DeliveryReport)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options)!;

        report.DeliveredAs.ShouldBe(expected);
    }

    [TestCase("international", AddressType.International)]
    [TestCase("national", AddressType.National)]
    [TestCase("shortcode", AddressType.Shortcode)]
    [TestCase("alphanumeric", AddressType.Alphanumeric)]
    public void Read_TextMessage_AllAddressTypes_AreParsed(string wireValue, AddressType expected)
    {
        var json = $$"""
                     {
                         "messageType": "text",
                         "notificationId": "n",
                         "senderAddress": "41791111111",
                         "senderAddressType": "{{wireValue}}",
                         "recipientAddress": "066012345678",
                         "recipientAddressType": "{{wireValue}}",
                         "textMessageContent": "Hi"
                     }
                     """;

        var text = (WebSmsWebhookRequest.Text)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options)!;

        text.SenderAddressType.ShouldBe(expected);
        text.RecipientAddressType.ShouldBe(expected);
    }

    [Test]
    public void Read_DeliveryReport_TimestampWithTimezone_PreservesOffset()
    {
        const string json = """
                            {
                                "messageType": "deliveryReport",
                                "notificationId": "n",
                                "transferId": "t",
                                "senderAddress": "41791111111",
                                "deliveryReportMessageStatus": "delivered",
                                "sentOn": "2013-05-27T13:36:00.000+02:00",
                                "deliveredOn": "2013-05-27T13:36:00.000+02:00"
                            }
                            """;

        var report = (WebSmsWebhookRequest.DeliveryReport)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options)!;

        report.SentOn.Offset.ShouldBe(TimeSpan.FromHours(2));
        report.SentOn.UtcDateTime.ShouldBe(new DateTime(2013, 5, 27, 11, 36, 0, DateTimeKind.Utc));
        report.DeliveredOn!.Value.Offset.ShouldBe(TimeSpan.FromHours(2));
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
    public void Read_NullMessageType_ThrowsJsonException()
    {
        const string json = """
                            {
                                "messageType": null,
                                "notificationId": "abc123",
                                "senderAddress": "4367612345678"
                            }
                            """;

        Should.Throw<JsonException>(() =>
            JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options));
    }

    [Test]
    public void Read_NumericMessageType_ThrowsJsonException()
    {
        const string json = """
                            {
                                "messageType": 42,
                                "notificationId": "abc123",
                                "senderAddress": "4367612345678"
                            }
                            """;

        Should.Throw<JsonException>(() =>
            JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options));
    }

    [Test]
    public void Read_NonObjectRoot_ThrowsJsonException()
    {
        Should.Throw<JsonException>(() =>
            JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>("[1,2,3]", _options));
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
    public void Read_EmptyMessageType_ThrowsJsonException()
    {
        const string json = """
                            {
                                "messageType": "",
                                "notificationId": "abc123",
                                "senderAddress": "4367612345678"
                            }
                            """;

        Should.Throw<JsonException>(() =>
            JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options));
    }

    [Test]
    public void Read_MessageTypeIsCaseSensitive_TitleCaseRejected()
    {
        const string json = """
                            {
                                "messageType": "Text",
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
    public void Write_BinaryMessage_SerializesWithMessageType()
    {
        var binary = new WebSmsWebhookRequest.Binary
        {
            MessageType = WebhookMessageType.Binary,
            NotificationId = "xyz789",
            SenderAddress = "4366012345678",
            SenderAddressType = AddressType.International,
            RecipientAddress = "066012345678",
            RecipientAddressType = AddressType.National,
            UserDataHeaderPresent = true,
            BinaryMessageContent = ["AQID", "BAUG"]
        };

        var json = JsonSerializer.Serialize<WebSmsWebhookRequest.Base>(binary, _options);

        json.ShouldContain("\"messageType\":\"binary\"");
        json.ShouldContain("\"userDataHeaderPresent\":true");
        json.ShouldContain("\"binaryMessageContent\":[\"AQID\",\"BAUG\"]");
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
    public void Write_DeliveryReport_NullOptionalFields_OmitsThemFromJson()
    {
        var report = new WebSmsWebhookRequest.DeliveryReport
        {
            MessageType = WebhookMessageType.DeliveryReport,
            NotificationId = "notif1",
            SenderAddress = "41791111111",
            TransferId = "transfer1",
            DeliveryReportMessageStatus = DeliveryReportMessageStatus.Undelivered,
            SentOn = DateTimeOffset.Parse("2024-10-15T20:33:02.000+02:00")
        };

        var json = JsonSerializer.Serialize<WebSmsWebhookRequest.Base>(report, _options);

        json.ShouldNotContain("\"deliveredOn\"");
        json.ShouldNotContain("\"deliveredAs\"");
        json.ShouldNotContain("\"clientMessageId\"");
    }

    [Test]
    public void Write_DeliveryReport_FailoverSms_SerializesAsHyphenatedValue()
    {
        var report = new WebSmsWebhookRequest.DeliveryReport
        {
            MessageType = WebhookMessageType.DeliveryReport,
            NotificationId = "notif1",
            SenderAddress = "41791111111",
            TransferId = "transfer1",
            DeliveryReportMessageStatus = DeliveryReportMessageStatus.Delivered,
            SentOn = DateTimeOffset.Parse("2024-10-15T20:33:02.000+02:00"),
            DeliveredOn = DateTimeOffset.Parse("2024-10-15T20:33:03.000+02:00"),
            DeliveredAs = DeliveredAs.FailoverSms
        };

        var json = JsonSerializer.Serialize<WebSmsWebhookRequest.Base>(report, _options);

        json.ShouldContain("\"deliveredAs\":\"failover-sms\"");
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
                                "messageFlashSms": true,
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
        var text = (WebSmsWebhookRequest.Text)back;
        text.TextMessageContent.ShouldBe("Round-trip test");
        text.MessageFlashSms.ShouldBeTrue();
        text.SenderAddressType.ShouldBe(AddressType.International);
        text.RecipientAddressType.ShouldBe(AddressType.National);
    }

    [Test]
    public void RoundTrip_BinaryMessage_PreservesAllFields()
    {
        const string json = """
                            {
                                "messageType": "binary",
                                "notificationId": "roundtripBin",
                                "senderAddress": "4366012345678",
                                "senderAddressType": "international",
                                "recipientAddress": "066012345678",
                                "recipientAddressType": "national",
                                "userDataHeaderPresent": true,
                                "binaryMessageContent": ["AQID", "BAUG"]
                            }
                            """;

        var deserialized = JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options);
        var reserialized = JsonSerializer.Serialize(deserialized, deserialized!.GetType(), _options);
        var back = (WebSmsWebhookRequest.Binary)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(reserialized, _options)!;

        back.NotificationId.ShouldBe("roundtripBin");
        back.UserDataHeaderPresent.ShouldBeTrue();
        back.BinaryMessageContent.ShouldBe(["AQID", "BAUG"]);
    }

    [Test]
    public void RoundTrip_DeliveryReport_PreservesAllFields()
    {
        const string json = """
                            {
                                "messageType": "deliveryReport",
                                "notificationId": "roundtripDr",
                                "transferId": "transfer-xyz",
                                "senderAddress": "41791111111",
                                "deliveryReportMessageStatus": "delivered",
                                "sentOn": "2024-10-15T20:33:02.000+02:00",
                                "deliveredOn": "2024-10-15T20:33:03.000+02:00",
                                "deliveredAs": "failover-sms",
                                "clientMessageId": "abc-123"
                            }
                            """;

        var deserialized = JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, _options);
        var reserialized = JsonSerializer.Serialize(deserialized, deserialized!.GetType(), _options);
        var back = (WebSmsWebhookRequest.DeliveryReport)JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(reserialized, _options)!;

        back.NotificationId.ShouldBe("roundtripDr");
        back.TransferId.ShouldBe("transfer-xyz");
        back.DeliveryReportMessageStatus.ShouldBe(DeliveryReportMessageStatus.Delivered);
        back.DeliveredOn.ShouldNotBeNull();
        back.DeliveredAs.ShouldBe(DeliveredAs.FailoverSms);
        back.ClientMessageId.ShouldBe("abc-123");
    }
}
