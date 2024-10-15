using System.Text;

namespace WebSmsNet.Abstractions.Configuration;

/// <summary>
/// Provides extension methods for configuring HttpClient with WebSms API options.
/// </summary>
public static class HttpClientExtension
{
    /// <summary>
    /// Applies the WebSms API options to the specified HttpClient.
    /// </summary>
    /// <param name="client">The HttpClient to configure.</param>
    /// <param name="options">The options to apply to the HttpClient.</param>
    /// <returns>The configured HttpClient.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required authentication information is missing or the authentication type is not supported.
    /// </exception>
    public static HttpClient ApplyWebSmsApiOptions(this HttpClient client, WebSmsApiOptions options)
    {
        client.BaseAddress = new(options.BaseUrl);
        client.DefaultRequestHeaders.Accept.Add(new("application/json"));

        switch (options.AuthenticationType)
        {
            case AuthenticationType.Basic:
                if (string.IsNullOrEmpty(options.Username) || string.IsNullOrEmpty(options.Password))
                    throw new InvalidOperationException("Username and password must be provided for Basic authentication.");

                client.DefaultRequestHeaders.Authorization = new("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.Username}:{options.Password}")));
                break;

            case AuthenticationType.Bearer:
                if (string.IsNullOrEmpty(options.AccessToken))
                    throw new InvalidOperationException("Access token must be provided for Bearer authentication.");

                client.DefaultRequestHeaders.Authorization = new("Bearer", options.AccessToken);
                break;

            default:
                throw new InvalidOperationException("Authentication type not supported.");
        }

        return client;
    }
}
