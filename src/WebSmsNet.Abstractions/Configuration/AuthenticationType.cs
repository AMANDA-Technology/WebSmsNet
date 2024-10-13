namespace WebSmsNet.Abstractions.Configuration;

/// <summary>
/// Specifies the basic authentication type that uses a username and password for access.
/// </summary>
public enum AuthenticationType
{
    /// <summary>
    /// Specifies the basic authentication type that uses a username and password for access.
    /// </summary>
    Basic,

    /// <summary>
    /// Specifies the bearer authentication type, which uses a token for access.
    /// </summary>
    Bearer
}
