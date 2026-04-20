using System.Text;
using Shouldly;
using WebSmsNet.Abstractions.Helpers;

namespace WebSmsNet.UnitTests.Helpers;

[TestFixture]
public class BinaryContentTests
{
    [Test]
    public void Parse_SinglePart_WithoutUdh_ReturnsDecodedText()
    {
        var text = "Hello";
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        var result = BinaryContent.Parse([base64], false);

        result.ShouldBe("Hello");
    }

    [Test]
    public void Parse_MultipleParts_WithoutUdh_ConcatenatesAll()
    {
        var part1 = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello "));
        var part2 = Convert.ToBase64String(Encoding.UTF8.GetBytes("World"));

        var result = BinaryContent.Parse([part1, part2], false);

        result.ShouldBe("Hello World");
    }

    [Test]
    public void Parse_SinglePart_WithUdh_StripsHeaderAndReturnsText()
    {
        // Header: [0x05, 0x00, 0x03, 0xCC, 0x01, 0x01] = 6 bytes (length byte 0x05 means skip 6)
        var header = new byte[] { 0x05, 0x00, 0x03, 0xCC, 0x01, 0x01 };
        var body = Encoding.UTF8.GetBytes("Hi");
        var encoded = Convert.ToBase64String(header.Concat(body).ToArray());

        var result = BinaryContent.Parse([encoded], true);

        result.ShouldBe("Hi");
    }

    [Test]
    public void Parse_EmptyList_ReturnsEmptyString()
    {
        var result = BinaryContent.Parse([], false);

        result.ShouldBe(string.Empty);
    }

    [Test]
    public void Parse_EmptyStringPart_WithUdh_ReturnsEmptyString()
    {
        // An empty Base64 string decodes to an empty byte array; SkipHeader must not crash on it.
        var result = BinaryContent.Parse([""], true);

        result.ShouldBe(string.Empty);
    }

    [Test]
    public void CreateMessageContentParts_SingleMessage_ReturnsSinglePart()
    {
        var parts = BinaryContent.CreateMessageContentParts("Hello").ToList();

        parts.Count.ShouldBe(1);
    }

    [Test]
    public void CreateMessageContentParts_MultipleMessages_ReturnsMatchingCount()
    {
        var parts = BinaryContent.CreateMessageContentParts("Part1", "Part2", "Part3").ToList();

        parts.Count.ShouldBe(3);
    }

    [Test]
    public void CreateMessageContentParts_SingleMessage_HasCorrectUdhHeader()
    {
        var parts = BinaryContent.CreateMessageContentParts("Test").ToList();
        var decoded = Convert.FromBase64String(parts[0]);

        // UDH: [0x05, 0x00, 0x03, 0xCC, totalParts=1, index=1]
        decoded[0].ShouldBe((byte)0x05);
        decoded[1].ShouldBe((byte)0x00);
        decoded[2].ShouldBe((byte)0x03);
        decoded[3].ShouldBe((byte)0xCC);
        decoded[4].ShouldBe((byte)1); // totalParts
        decoded[5].ShouldBe((byte)1); // index
    }

    [Test]
    public void CreateMessageContentParts_MultipleMessages_HasCorrectTotalPartsInHeader()
    {
        var parts = BinaryContent.CreateMessageContentParts("A", "B", "C").ToList();

        foreach (var part in parts)
        {
            var decoded = Convert.FromBase64String(part);
            decoded[4].ShouldBe((byte)3); // totalParts
        }
    }

    [Test]
    public void CreateMessageContentParts_MultipleMessages_HasCorrectIndexInHeader()
    {
        var parts = BinaryContent.CreateMessageContentParts("A", "B", "C").ToList();

        for (var i = 0; i < parts.Count; i++)
        {
            var decoded = Convert.FromBase64String(parts[i]);
            decoded[5].ShouldBe((byte)(i + 1)); // index is 1-based
        }
    }

    [Test]
    public void RoundTrip_SingleMessage_PreservesContent()
    {
        var parts = BinaryContent.CreateMessageContentParts("Hello World").ToList();

        var result = BinaryContent.Parse(parts, true);

        result.ShouldBe("Hello World");
    }

    [Test]
    public void RoundTrip_MultipleMessages_ConcatenatesCorrectly()
    {
        var parts = BinaryContent.CreateMessageContentParts("hi there! ", "this is a test message ", "with 3 sms.").ToList();

        var result = BinaryContent.Parse(parts, true);

        result.ShouldBe("hi there! this is a test message with 3 sms.");
    }

    [Test]
    public void RoundTrip_UnicodeContent_PreservesContent()
    {
        var parts = BinaryContent.CreateMessageContentParts("Ümläute & €uro").ToList();

        var result = BinaryContent.Parse(parts, true);

        result.ShouldBe("Ümläute & €uro");
    }
}
