using System.Text;
using Shouldly;
using WebSmsNet.Abstractions.Configuration;

namespace WebSmsNet.UnitTests.Configuration;

[TestFixture]
public class HttpClientExtensionTests
{
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _client = new HttpClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    [Test]
    public void ApplyWebSmsApiOptions_Bearer_SetsBaseAddress()
    {
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Bearer,
            AccessToken = "mytoken"
        };

        _client.ApplyWebSmsApiOptions(options);

        _client.BaseAddress.ShouldBe(new Uri("https://api.example.com/"));
    }

    [Test]
    public void ApplyWebSmsApiOptions_Bearer_SetsAcceptHeader()
    {
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Bearer,
            AccessToken = "mytoken"
        };

        _client.ApplyWebSmsApiOptions(options);

        _client.DefaultRequestHeaders.Accept
            .ShouldContain(h => h.MediaType == "application/json");
    }

    [Test]
    public void ApplyWebSmsApiOptions_Bearer_SetsAuthorizationHeader()
    {
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Bearer,
            AccessToken = "my-access-token"
        };

        _client.ApplyWebSmsApiOptions(options);

        _client.DefaultRequestHeaders.Authorization.ShouldNotBeNull();
        _client.DefaultRequestHeaders.Authorization!.Scheme.ShouldBe("Bearer");
        _client.DefaultRequestHeaders.Authorization.Parameter.ShouldBe("my-access-token");
    }

    [Test]
    public void ApplyWebSmsApiOptions_Bearer_ReturnsClient()
    {
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Bearer,
            AccessToken = "token"
        };

        var result = _client.ApplyWebSmsApiOptions(options);

        result.ShouldBeSameAs(_client);
    }

    [Test]
    public void ApplyWebSmsApiOptions_Bearer_MissingToken_ThrowsInvalidOperationException()
    {
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Bearer,
            AccessToken = null
        };

        Should.Throw<InvalidOperationException>(() =>
            _client.ApplyWebSmsApiOptions(options));
    }

    [Test]
    public void ApplyWebSmsApiOptions_Bearer_EmptyToken_ThrowsInvalidOperationException()
    {
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Bearer,
            AccessToken = ""
        };

        Should.Throw<InvalidOperationException>(() =>
            _client.ApplyWebSmsApiOptions(options));
    }

    [Test]
    public void ApplyWebSmsApiOptions_Basic_SetsBaseAddress()
    {
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Basic,
            Username = "user",
            Password = "pass"
        };

        _client.ApplyWebSmsApiOptions(options);

        _client.BaseAddress.ShouldBe(new Uri("https://api.example.com/"));
    }

    [Test]
    public void ApplyWebSmsApiOptions_Basic_SetsAuthorizationHeader()
    {
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Basic,
            Username = "testuser",
            Password = "testpass"
        };

        _client.ApplyWebSmsApiOptions(options);

        var expected = Convert.ToBase64String(Encoding.ASCII.GetBytes("testuser:testpass"));
        _client.DefaultRequestHeaders.Authorization.ShouldNotBeNull();
        _client.DefaultRequestHeaders.Authorization!.Scheme.ShouldBe("Basic");
        _client.DefaultRequestHeaders.Authorization.Parameter.ShouldBe(expected);
    }

    [Test]
    public void ApplyWebSmsApiOptions_Basic_MissingUsername_ThrowsInvalidOperationException()
    {
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Basic,
            Username = null,
            Password = "pass"
        };

        Should.Throw<InvalidOperationException>(() =>
            _client.ApplyWebSmsApiOptions(options));
    }

    [Test]
    public void ApplyWebSmsApiOptions_Basic_EmptyUsername_ThrowsInvalidOperationException()
    {
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Basic,
            Username = "",
            Password = "pass"
        };

        Should.Throw<InvalidOperationException>(() =>
            _client.ApplyWebSmsApiOptions(options));
    }

    [Test]
    public void ApplyWebSmsApiOptions_Basic_MissingPassword_ThrowsInvalidOperationException()
    {
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Basic,
            Username = "user",
            Password = null
        };

        Should.Throw<InvalidOperationException>(() =>
            _client.ApplyWebSmsApiOptions(options));
    }

    [Test]
    public void ApplyWebSmsApiOptions_Basic_EmptyPassword_ThrowsInvalidOperationException()
    {
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = AuthenticationType.Basic,
            Username = "user",
            Password = ""
        };

        Should.Throw<InvalidOperationException>(() =>
            _client.ApplyWebSmsApiOptions(options));
    }

    [Test]
    public void ApplyWebSmsApiOptions_UnsupportedAuthType_ThrowsInvalidOperationException()
    {
        var options = new WebSmsApiOptions
        {
            BaseUrl = "https://api.example.com/",
            AuthenticationType = (AuthenticationType)99
        };

        Should.Throw<InvalidOperationException>(() =>
            _client.ApplyWebSmsApiOptions(options));
    }
}
