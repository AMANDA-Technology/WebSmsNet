using System.Diagnostics.CodeAnalysis;

namespace WebSmsNet.Abstractions.Configuration;

/// <summary>
/// Represents the configuration options for the WebSms API.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class WebSmsApiOptions
{
    /// <summary>
    /// Gets or sets the base URL for the WebSms API.
    /// </summary>
    public required string BaseUrl { get; init; }

    /// <summary>
    /// Specifies the type of authentication to use with the WebSms API.
    /// </summary>
    public AuthenticationType AuthenticationType { get; init; }

    /// <summary>
    /// Gets or sets the access token used for bearer authentication in the WebSms API.
    /// </summary>
    public string? AccessToken { get; init; }

    /// <value>
    /// Gets or sets the username for authentication when using Basic authentication type.
    /// </value>
    public string? Username { get; init; }

    /// <summary>
    /// Gets or sets the password for user-based authentication.
    /// </summary>
    /// <remarks>
    /// This property is used in conjunction with the <see cref="Username"/> property
    /// to authenticate requests to the WebSms API when <see cref="AuthenticationType"/>
    /// is set to <see cref="AuthenticationType.Basic"/>.
    /// </remarks>
    public string? Password { get; init; }
}
