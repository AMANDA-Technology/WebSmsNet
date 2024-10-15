using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.Abstractions.Serialization;

namespace WebSmsNet.AspNetCore.Helpers;

/// <summary>
/// Provides helper methods for parsing websms webhook requests.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class WebSmsWebhook
{
    /// <summary>
    /// Parses a JSON string into a <see cref="WebSmsWebhookRequest.Base"/> object.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>The parsed <see cref="WebSmsWebhookRequest.Base"/> object if parsing succeeds, otherwise null.</returns>
    public static WebSmsWebhookRequest.Base Parse(string json) =>
        JsonSerializer.Deserialize<WebSmsWebhookRequest.Base>(json, WebSmsJsonSerialization.DefaultOptions)
        ?? throw new InvalidOperationException("Failed to deserialize web sms webhook request");

    /// <summary>
    /// Parses the body of an incoming HTTP request into a <see cref="WebSmsWebhookRequest.Base"/> object.
    /// </summary>
    /// <param name="requestStream">The stream representing the body of the HTTP request.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>The parsed <see cref="WebSmsWebhookRequest.Base"/> object if parsing succeeds.</returns>
    public static async Task<WebSmsWebhookRequest.Base> Parse(Stream requestStream, [Optional] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(requestStream);
        return Parse(await reader.ReadToEndAsync(cancellationToken));
    }

    /// <summary>
    /// Matches the webhook request to a specific handler based on the request type and invokes the corresponding handler function.
    /// </summary>
    /// <param name="webhookRequest">The webhook request to process.</param>
    /// <param name="onText">The handler function to invoke if the request is of type <see cref="WebSmsWebhookRequest.Text"/>.</param>
    /// <param name="onBinary">The handler function to invoke if the request is of type <see cref="WebSmsWebhookRequest.Binary"/>.</param>
    /// <param name="onDeliveryReport">The handler function to invoke if the request is of type <see cref="WebSmsWebhookRequest.DeliveryReport"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the request type is unknown.</exception>
    public static T Match<T>(this WebSmsWebhookRequest.Base webhookRequest, Func<WebSmsWebhookRequest.Text, T> onText, Func<WebSmsWebhookRequest.Binary, T> onBinary, Func<WebSmsWebhookRequest.DeliveryReport, T> onDeliveryReport) =>
        webhookRequest switch
        {
            WebSmsWebhookRequest.Text text => onText(text),
            WebSmsWebhookRequest.Binary binary => onBinary(binary),
            WebSmsWebhookRequest.DeliveryReport deliveryReport => onDeliveryReport(deliveryReport),
            _ => throw new ArgumentOutOfRangeException(nameof(webhookRequest), webhookRequest.GetType().FullName, "Unknown WebSmsWebhookRequest type")
        };

    /// <summary>
    /// Matches the webhook request to a specific handler based on the request type and invokes the corresponding handler function.
    /// </summary>
    /// <param name="webhookRequest">The webhook request to process.</param>
    /// <param name="onText">The handler function to invoke if the request is of type <see cref="WebSmsWebhookRequest.Text"/>.</param>
    /// <param name="onBinary">The handler function to invoke if the request is of type <see cref="WebSmsWebhookRequest.Binary"/>.</param>
    /// <param name="onDeliveryReport">The handler function to invoke if the request is of type <see cref="WebSmsWebhookRequest.DeliveryReport"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the request type is unknown.</exception>
    public static void Match(this WebSmsWebhookRequest.Base webhookRequest, Action<WebSmsWebhookRequest.Text> onText, Action<WebSmsWebhookRequest.Binary> onBinary, Action<WebSmsWebhookRequest.DeliveryReport> onDeliveryReport)
    {
        switch (webhookRequest)
        {
            case WebSmsWebhookRequest.Text request:
                onText(request);
                return;

            case WebSmsWebhookRequest.Binary binary:
                onBinary(binary);
                return;

            case WebSmsWebhookRequest.DeliveryReport deliveryReport:
                onDeliveryReport(deliveryReport);
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(webhookRequest), webhookRequest.GetType().FullName, "Unknown WebSmsWebhookRequest type");
        }
    }
}
