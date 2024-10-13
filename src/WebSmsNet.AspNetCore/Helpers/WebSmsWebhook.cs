using System.Text.Json;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.Abstractions.Serialization;

namespace WebSmsNet.AspNetCore.Helpers;

/// <summary>
/// Provides helper methods for parsing websms webhook requests.
/// </summary>
public static class WebSmsWebhook
{
    /// <summary>
    /// Parses a JSON string into a <see cref="WebSmsWebhookRequest.Base"/> object.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>The parsed <see cref="WebSmsWebhookRequest.Base"/> object if parsing succeeds, otherwise null.</returns>
    public static WebSmsWebhookRequest.Base? Parse(string json) =>
        JsonSerializer.Deserialize<WebSmsWebhookRequest.Text>(json, WebSmsJsonSerialization.DefaultOptions);
}
