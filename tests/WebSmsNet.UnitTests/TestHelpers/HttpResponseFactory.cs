using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebSmsNet.Abstractions.Models;
using WebSmsNet.Abstractions.Models.Enums;
using WebSmsNet.Abstractions.Serialization;

namespace WebSmsNet.UnitTests.TestHelpers;

/// <summary>
/// Factory helpers that produce canned <see cref="HttpResponseMessage"/> instances for tests.
/// </summary>
internal static class HttpResponseFactory
{
    public static MessageSendResponse SampleResponse(string clientMessageId = "client-123", int smsCount = 1) => new()
    {
        ClientMessageId = clientMessageId,
        SmsCount = smsCount,
        StatusCode = WebSmsStatusCode.Ok,
        StatusMessage = "OK",
        TransferId = "transfer-xyz"
    };

    public static HttpResponseMessage JsonOk(MessageSendResponse body) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(body, WebSmsJsonSerialization.DefaultOptions),
                Encoding.UTF8)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            }
        };

    public static HttpResponseMessage RawJson(HttpStatusCode statusCode, string json) =>
        new(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            }
        };
}
