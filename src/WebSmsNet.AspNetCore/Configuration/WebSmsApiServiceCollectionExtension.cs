using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebSmsNet.Abstractions;
using WebSmsNet.Abstractions.Configuration;

namespace WebSmsNet.AspNetCore.Configuration;

/// <summary>
/// Provides extension methods for IServiceCollection to add websms API services.
/// </summary>
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
public static class WebSmsApiServiceCollectionExtension
{
    /// <summary>
    /// Adds the websms API client to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configureOptions">An action to configure the websms options.</param>
    /// <returns>The original IServiceCollection, with the websms api client added.</returns>
    public static IServiceCollection AddWebSmsApiClient(this IServiceCollection services, Action<WebSmsApiOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddHttpClient<WebSmsApiConnectionHandler>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<WebSmsApiOptions>>().Value;
            client.ApplyWebSmsApiOptions(options);
        });

        return services.AddScoped<IWebSmsApiClient>(provider =>
            new WebSmsApiClient(provider.GetRequiredService<WebSmsApiConnectionHandler>()));
    }
}
