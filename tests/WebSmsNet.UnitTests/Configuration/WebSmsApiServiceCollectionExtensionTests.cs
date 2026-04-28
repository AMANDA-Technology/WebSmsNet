using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using WebSmsNet.Abstractions;
using WebSmsNet.Abstractions.Configuration;
using WebSmsNet.AspNetCore.Configuration;

namespace WebSmsNet.UnitTests.Configuration;

[TestFixture]
public class WebSmsApiServiceCollectionExtensionTests
{
    [Test]
    public void AddWebSmsApiClient_RegistersWebSmsApiOptions()
    {
        var services = new ServiceCollection();

        services.AddWebSmsApiClient(opt =>
        {
            opt.BaseUrl = "https://api.example.com/";
            opt.AuthenticationType = AuthenticationType.Bearer;
            opt.AccessToken = "test-token";
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<WebSmsApiOptions>>().Value;
        options.BaseUrl.ShouldBe("https://api.example.com/");
        options.AuthenticationType.ShouldBe(AuthenticationType.Bearer);
        options.AccessToken.ShouldBe("test-token");
    }

    [Test]
    public void AddWebSmsApiClient_RegistersWebSmsApiConnectionHandlerAsTypedHttpClient()
    {
        var services = new ServiceCollection();

        services.AddWebSmsApiClient(opt =>
        {
            opt.BaseUrl = "https://api.example.com/";
            opt.AuthenticationType = AuthenticationType.Bearer;
            opt.AccessToken = "test-token";
        });

        using var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<WebSmsApiConnectionHandler>();
        handler.ShouldNotBeNull();
    }

    [Test]
    public void AddWebSmsApiClient_RegistersIWebSmsApiClientAsScoped()
    {
        var services = new ServiceCollection();

        services.AddWebSmsApiClient(opt =>
        {
            opt.BaseUrl = "https://api.example.com/";
            opt.AuthenticationType = AuthenticationType.Bearer;
            opt.AccessToken = "test-token";
        });

        var descriptor = services.Single(d => d.ServiceType == typeof(IWebSmsApiClient));
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Test]
    public void AddWebSmsApiClient_ResolvesIWebSmsApiClient()
    {
        var services = new ServiceCollection();

        services.AddWebSmsApiClient(opt =>
        {
            opt.BaseUrl = "https://api.example.com/";
            opt.AuthenticationType = AuthenticationType.Bearer;
            opt.AccessToken = "test-token";
        });

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IWebSmsApiClient>();
        client.ShouldNotBeNull();
    }

    [Test]
    public void AddWebSmsApiClient_ResolvedClientIsWebSmsApiClient()
    {
        var services = new ServiceCollection();

        services.AddWebSmsApiClient(opt =>
        {
            opt.BaseUrl = "https://api.example.com/";
            opt.AuthenticationType = AuthenticationType.Bearer;
            opt.AccessToken = "test-token";
        });

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IWebSmsApiClient>();
        client.ShouldBeOfType<WebSmsApiClient>();
    }

    [Test]
    public void AddWebSmsApiClient_ScopedLifetime_DifferentScopesYieldDifferentInstances()
    {
        var services = new ServiceCollection();

        services.AddWebSmsApiClient(opt =>
        {
            opt.BaseUrl = "https://api.example.com/";
            opt.AuthenticationType = AuthenticationType.Bearer;
            opt.AccessToken = "test-token";
        });

        using var provider = services.BuildServiceProvider();
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();
        var client1 = scope1.ServiceProvider.GetRequiredService<IWebSmsApiClient>();
        var client2 = scope2.ServiceProvider.GetRequiredService<IWebSmsApiClient>();

        client1.ShouldNotBeSameAs(client2);
    }

    [Test]
    public void AddWebSmsApiClient_AppliesOptionsToHttpClient()
    {
        var services = new ServiceCollection();

        services.AddWebSmsApiClient(opt =>
        {
            opt.BaseUrl = "https://api.example.com/";
            opt.AuthenticationType = AuthenticationType.Bearer;
            opt.AccessToken = "test-token";
        });

        using var provider = services.BuildServiceProvider();
        Should.NotThrow(() => provider.GetRequiredService<WebSmsApiConnectionHandler>());
    }

    [Test]
    public void AddWebSmsApiClient_ReturnsServiceCollectionForChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddWebSmsApiClient(opt =>
        {
            opt.BaseUrl = "https://api.example.com/";
            opt.AuthenticationType = AuthenticationType.Bearer;
            opt.AccessToken = "test-token";
        });

        result.ShouldBeSameAs(services);
    }
}
