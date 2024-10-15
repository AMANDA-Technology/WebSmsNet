# WebSmsNet

Unofficial API client implementation for the linkmobility websms Messaging API 1.0.0. See [API Doc](https://developer.linkmobility.eu/sms-api/rest-api).

With a special thanks to LINK Mobility Austria GmbH for the handy messaging service. See [websms website](https://www.websms.com/).

## Packages

- WebSmsNet: Client service to interact with the Messaging API 1.0.0
- WebSmsNet.Abstractions: Models, Views and Enums used for the API
- WebSmsNet.AspNetCore: Dependency injection in ASP.NET Core

[![BuildNuGetAndPublish](https://github.com/AMANDA-Technology/WebSmsNet/actions/workflows/main.yml/badge.svg)](https://github.com/AMANDA-Technology/WebSmsNet/actions/workflows/main.yml)

[![CodeQL](https://github.com/AMANDA-Technology/WebSmsNet/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/AMANDA-Technology/WebSmsNet/actions/workflows/codeql-analysis.yml)

[![SonarCloud](https://github.com/AMANDA-Technology/WebSmsNet/actions/workflows/sonar-analysis.yml/badge.svg)](https://github.com/AMANDA-Technology/WebSmsNet/actions/workflows/sonar-analysis.yml)

## Usage

### Configuration

For every type of setup, you need `WebSmsApiOptions` and you can choose how to authenticate...

- with access token:
    ```csharp
    new WebSmsApiOptions()
    {
        BaseUrl = "https://api.linkmobility.eu/";
        AuthenticationType = AuthenticationType.Bearer;
        AccessToken = "YOUR ACCESS_TOKEN";
    });
    ```
- or basic login:
    ```csharp
    new WebSmsApiOptions()
    {
        BaseUrl = "https://api.linkmobility.eu/";
        AuthenticationType = AuthenticationType.Basic;
        Username = "YOUR USER_NAME";
        Password = "YOUR PASSWORD";
    });
    ```

### Construct manually:

The `WebSmsApiClient` can be construction by passing either `WebSmsApiOptions`, `WebSmsApiConnectionHandler` or `HttpClient`.

- Simplest approach with options:
    ```csharp
    var webSmsApiClient = new WebSmsApiClient(/* your options */);
    ```
- Or you can pass a custom connection handler (must derive from `WebSmsApiConnectionHandler`):
    ```csharp
    var webSmsApiClient = new WebSmsApiClient(/* you connection handler */);
    ```
- Or if you need to adjust something on the `HttpClient` you can prepare it with the options:
    ```csharp
    var httpClient = new HttpClient().ApplyWebSmsApiOptions(/* your options */);
    /*
        Adjust the http client as needed
    */
    var webSmsApiClient = new WebSmsApiClient(httpClient);
    ```

### Dependency injection:

1. Register the client
    ```csharp
    builder.Services.AddWebSmsApiClient(options =>
    {
       options.BaseUrl = "https://api.linkmobility.eu/";
       options.AuthenticationType = AuthenticationType.Bearer;
       options.AccessToken = "YOUR ACCESS_TOKEN";
    });
    ```
2. Then inject `IWebSmsApiClient` it into your services.
    ```csharp
    public YourService(IWebSmsApiClient webSmsApiClient) 
    {
        // your code
    }
    ```

### Sending a message:

Call the client with wither `SendTextMessage` or `SendBinaryMessage`.
```csharp
webSmsApiClient.Messaging.SendTextMessage(new()
{
   RecipientAddressList =
   [
       "YOUR recipient's MSISDN"
   ],
   MessageContent = "hi there! this is a test message."
});
```

### Handle webhook requests

Use `WebSmsWebhook` helper class to parse the webhook request.
There are two options, both returning the base type which can be checked manually or matched with the extension method as follows.
`Match` supports simple actions and functions with a return value as well.

- Parse json string
    ```csharp
    var result = WebSmsWebhook.Parse(json).Match(
        onText: _ => /* your code */,
        onBinary: _ => /* your code */,
        onDeliveryReport: _ => /* your code */);
    ```
- Or parse http request body
    ```csharp
    var result = (await WebSmsWebhook.Parse(Request.Body)).Match(
        onText: _ => /* your code */,
        onBinary: _ => /* your code */,
        onDeliveryReport: _ => /* your code */);
    ```

### Custom connection handler

You can implement a custom websms API connection handler by deriving from `WebSmsApiConnectionHandler`.
By doing this you can either decorate or intercept the `Post` method with the provided virtual properties i.e. for auditing or other use cases.

```csharp
public class CustomWebSmsApiConnectionHandler(HttpClient httpClient) : WebSmsApiConnectionHandler(httpClient)
{
    protected override JsonSerializerOptions SerializerOptions => /* Your custom serializer options, or use and manipulate from base */;

    protected override Func<string, object, CancellationToken, Task> OnBeforePost =>
        (endpoint, data, cancellationToken) => /* Your function */;

    protected override Func<HttpResponseMessage, CancellationToken, Task<HttpResponseMessage>> OnResponseReceived =>
        (response, cancellationToken) => /* Your function (must return the http response message) */;

    protected override Action<HttpResponseMessage> EnsureSuccess =>
        (response) => /* Your custom validation */;

    public override async Task<T> Post<T>(string endpoint, object data, [Optional] CancellationToken cancellationToken)
    {
        /* Do something before */

        // Call the base function
        var response = await base.Post<T>(endpoint, data, cancellationToken);

        /* Do something after */
        return response;
    }
}
```
