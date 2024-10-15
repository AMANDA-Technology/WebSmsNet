using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using WebSmsNet.Abstractions.Serialization;

namespace WebSmsNet;

/// <summary>
/// Handles connections to the websms API.
/// </summary>
/// <param name="httpClient">The HTTP client used for making requests.</param>
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
public class WebSmsApiConnectionHandler(HttpClient httpClient)
{
    /// <summary>
    /// Gets the JsonSerializerOptions used for configuring the JSON serialization settings when communicating with the websms API.
    /// </summary>
    protected virtual JsonSerializerOptions SerializerOptions => WebSmsJsonSerialization.DefaultOptions;

    /// <summary>
    /// Gets or sets a function to be called before making a POST request to the websms API.
    /// Provides the endpoint string, the request data object, and a cancellation token.
    /// </summary>
    /// <returns>A task representing the pre-post operation.</returns>
    protected virtual Func<string, object, CancellationToken, Task> OnBeforePost => (_, _, _) => Task.CompletedTask;

    /// <summary>
    /// Gets or sets a function to be called when a response is received from the websms API.
    /// Provides the HTTP response message and a cancellation token.
    /// This is being called before <see cref="EnsureSuccess"/>.
    /// </summary>
    /// <returns>A task representing the response handling operation.</returns>
    protected virtual Func<HttpResponseMessage, CancellationToken, Task<HttpResponseMessage>> OnResponseReceived => (response, _) => Task.FromResult(response);

    /// <summary>
    /// A function invoked to ensure the HTTP response is successful.
    /// This typically involves calling `HttpResponseMessage.EnsureSuccessStatusCode` to throw an exception if the response indicates an unsuccessful status code.
    /// </summary>
    protected virtual Action<HttpResponseMessage> EnsureSuccess => response => response.EnsureSuccessStatusCode();

    /// <summary>
    /// Sends a POST request to the specified endpoint with the provided data.
    /// </summary>
    /// <param name="endpoint">The API endpoint to which the request is sent.</param>
    /// <param name="data">The data to be included in the POST request body.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <typeparam name="T">The type of the response expected from the API.</typeparam>
    /// <returns>A task representing the asynchronous operation, with a result of type <typeparamref name="T"/> containing the response from the API.</returns>
    public virtual async Task<T> Post<T>(string endpoint, object data, [Optional] CancellationToken cancellationToken)
    {
        await OnBeforePost(endpoint, data, cancellationToken);

        var response = await OnResponseReceived(
            await httpClient.PostAsJsonAsync(endpoint, data, SerializerOptions, cancellationToken),
            cancellationToken);

        EnsureSuccess(response);

        return await response.Content.ReadFromJsonAsync<T>(SerializerOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response.");
    }
}
