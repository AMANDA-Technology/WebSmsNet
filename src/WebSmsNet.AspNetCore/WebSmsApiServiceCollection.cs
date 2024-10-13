using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebSmsNet.Abstractions;
using WebSmsNet.Abstractions.Configuration;

namespace WebSmsNet.AspNetCore;

/// <summary>
/// Provides extension methods for IServiceCollection to add websms API services.
/// </summary>
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class WebSmsApiServiceCollection
{
    /// <summary>
    /// Adds the websms API client to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configureOptions">An action to configure the websms options.</param>
    /// <returns>The original IServiceCollection, with the websms api client added.</returns>
    public static IServiceCollection WebSmsApiClient(this IServiceCollection services, Action<WebSmsApiOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddHttpClient<WebSmsApiConnectionHandler>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<WebSmsApiOptions>>().Value;
            client.ApplyWebSmsApiOptions(options);
        });

        return services.AddScoped<IWebSmsApiClient, WebSmsApiClient>();
    }

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
