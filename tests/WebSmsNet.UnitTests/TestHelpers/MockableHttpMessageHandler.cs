namespace WebSmsNet.UnitTests.TestHelpers;

/// <summary>
/// Abstract HttpMessageHandler exposing a public abstract method that wraps the
/// protected <see cref="HttpMessageHandler.SendAsync"/>. Allows NSubstitute to
/// intercept HTTP traffic without crossing the protected access boundary.
/// </summary>
public abstract class MockableHttpMessageHandler : HttpMessageHandler
{
    public abstract Task<HttpResponseMessage> SendAsyncMock(HttpRequestMessage request, CancellationToken cancellationToken);

    protected sealed override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => SendAsyncMock(request, cancellationToken);
}
