# WebSmsNet

Unofficial API client implementation for the linkmobility websms Messaging API 1.0.0. See [API Doc](https://developer.linkmobility.eu/sms-api/rest-api).

With a special thanks to LINK Mobility Austria GmbH for the handy messaging service. See [websms website](https://www.websms.com/).

### Packages

- WebSmsNet: Client service to interact with the Messaging API 1.0.0
- WebSmsNet.Abstractions: Models, Views and Enums used for the API
- WebSmsNet.AspNetCore: Dependency injection in ASP.NET Core

[![BuildNuGetAndPublish](https://github.com/AMANDA-Technology/WebSmsNet/actions/workflows/main.yml/badge.svg)](https://github.com/AMANDA-Technology/WebSmsNet/actions/workflows/main.yml)

[![CodeQL](https://github.com/AMANDA-Technology/WebSmsNet/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/AMANDA-Technology/WebSmsNet/actions/workflows/codeql-analysis.yml)

[![SonarCloud](https://github.com/AMANDA-Technology/WebSmsNet/actions/workflows/sonar-analysis.yml/badge.svg)](https://github.com/AMANDA-Technology/WebSmsNet/actions/workflows/sonar-analysis.yml)

### Usage

#### Dependency injection:

1. Register the client.
    ```c#
    builder.Services.WebSmsApiClient(options =>
    {
        options.AuthenticationType = AuthenticationType.Bearer;
        options.AccessToken = "YOUR ACCESS_TOKEN";
    });
    ```

2. Then inject `IWebSmsApiClient` it into your services.

#### Sending a text message:

Call the client.

```c#
webSmsApiClient.Messaging.SendTextMessageAsync(new()
{
   RecipientAddressList =
   [
       "YOUR Recipients MSISDN"
   ],
   MessageContent = "hi there! this is a test message."
});
```
