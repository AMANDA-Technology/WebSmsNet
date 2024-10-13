using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebSmsNet;

/// <summary>
/// Handles connections to the websms API.
/// </summary>
/// <param name="httpClient">The HTTP client used for making requests.</param>
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class WebSmsApiConnectionHandler(HttpClient httpClient)
{
    /// <summary>
    /// Gets or sets a function to be called when a response is received from the websms API.
    /// </summary>
    protected virtual Func<HttpResponseMessage, Task> OnResponseReceived => _ => Task.CompletedTask;

    /// <summary>
    /// Gets the JsonSerializerOptions used for configuring the JSON serialization settings when communicating with the websms API.
    /// </summary>
    protected virtual JsonSerializerOptions SerializerOptions => new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    /// <summary>
    /// Sends a POST request to the specified endpoint with the provided data.
    /// </summary>
    /// <param name="endpoint">The API endpoint to which the request is sent.</param>
    /// <param name="data">The data to be included in the POST request body.</param>
    /// <typeparam name="T">The type of the response expected from the API.</typeparam>
    /// <returns>A task representing the asynchronous operation, with a result of type <typeparamref name="T"/> containing the response from the API.</returns>
    public async Task<T> PostAsync<T>(string endpoint, object data)
    {
        var response = await httpClient.PostAsJsonAsync(endpoint, data, SerializerOptions);
        response.EnsureSuccessStatusCode();

        await OnResponseReceived(response);

        return await response.Content.ReadFromJsonAsync<T>(SerializerOptions)
               ?? throw new InvalidOperationException("Failed to deserialize response.");
    }
}
