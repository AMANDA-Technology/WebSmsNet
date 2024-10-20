using System.Text;

namespace WebSmsNet.Abstractions.Helpers;

/// <summary>
/// Helper for binary content
/// </summary>
public static class BinaryContent
{
    /// <summary>
    /// Parse binary message content
    /// </summary>
    /// <param name="binaryMessageContent"></param>
    /// <param name="userDataHeaderPresent"></param>
    /// <returns></returns>
    public static string Parse(List<string> binaryMessageContent, bool userDataHeaderPresent)
    {
        var messageBuilder = new StringBuilder();

        foreach (var messageContent in binaryMessageContent
                     .Select(Convert.FromBase64String)
                     .Select(binaryData => userDataHeaderPresent ? SkipHeader(binaryData) : binaryData))
        {
            messageBuilder.Append(Encoding.UTF8.GetString(messageContent));
        }

        return messageBuilder.ToString();
    }

    private static byte[] SkipHeader(byte[] binaryData) =>
        binaryData.Skip(binaryData[0] + 1).ToArray();

    /// <summary>
    /// Create binary message content (List of Base64 encoded message parts)
    /// </summary>
    /// <param name="messageTexts"></param>
    /// <returns></returns>
    public static IEnumerable<string> CreateMessageContentParts(params string[] messageTexts) =>
        messageTexts
            .Select((messageText, index) =>
                CreateMessagePart(messageTexts.Length, index + 1, messageText));

    private static string CreateMessagePart(int totalParts, int index, string messageText) =>
        Convert.ToBase64String(
            Header(totalParts, index)
                .Concat(Encoding.UTF8.GetBytes(messageText))
                .ToArray());

    // https://en.wikipedia.org/wiki/Concatenated_SMS
    private static byte[] Header(int totalParts, int index) =>
        [
            0x05, // UDH length
            0x00, // Information Element Identifier
            0x03, // Information Element Data Length
            0xCC, // CSMS reference number
            (byte)totalParts,
            (byte)index
        ];
}
