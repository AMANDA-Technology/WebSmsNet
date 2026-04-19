---
title: WebSmsNet - Claude Code Instructions
tags: [claude, onboarding, dotnet, api-client, sms, linkmobility, websms]
---

# WebSmsNet

Unofficial .NET client library for the [LINK Mobility websms Messaging REST API 1.0.0](https://developer.linkmobility.eu/sms-api/rest-api). The library provides a typed C# client for sending text and binary SMS, parsing inbound webhook notifications (text, binary, delivery report), and easy registration through ASP.NET Core dependency injection. It ships as three NuGet packages: `WebSmsNet`, `WebSmsNet.Abstractions`, and `WebSmsNet.AspNetCore`.

> **AI agents:** strict procedural rules live in [`ai_instructions.md`](./ai_instructions.md). That file is the contract for agent behavior and overrides this file where they disagree.

## Tech Stack

| Layer            | Technology                                                                   |
| ---------------- | ---------------------------------------------------------------------------- |
| Language         | C# 13 (`<LangVersion>13</LangVersion>`)                                      |
| Framework        | .NET 9 (`net9.0`)                                                            |
| HTTP             | `System.Net.Http.HttpClient` via `IHttpClientFactory` (ASP.NET Core package) |
| Serialization    | `System.Text.Json` (web defaults + camel-case enum converter)                |
| DI integration   | ASP.NET Core (`Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Http`) |
| Testing          | xUnit 2.9 + FluentAssertions 6.12 + Coverlet (`WebSmsNet.Tests`)             |
| Build            | MSBuild (SDK-style csproj) — `GeneratePackageOnBuild` + `GenerateDocumentationFile` |
| CI/CD            | GitHub Actions (`main.yml`, `codeql-analysis.yml`, `sonar-analysis.yml`)     |
| License          | MIT                                                                          |

> Note: the testing stack (xUnit + FluentAssertions) differs from the AMANDA-Technology org default (NUnit + Shouldly). Match the **existing** project style when adding tests here — do not migrate the test framework as a side-effect of another change.

## Solution Structure

```
WebSmsNet.sln
  src/
    WebSmsNet.Abstractions/            -- Models, enums, serializers, DI-agnostic helpers (NuGet package)
      Configuration/                     -- WebSmsApiOptions, AuthenticationType, HttpClient extension
      Connectors/                        -- Connector interfaces (IMessagingConnector)
      Helpers/                           -- BinaryContent helper (UDH-aware Base64 splitter/joiner)
      Models/                            -- Request/response DTOs and webhook models
        Enums/                           -- Public enums (WebSmsStatusCode, AddressType, MessageType, ...)
      Serialization/                     -- WebSmsJsonSerialization, WebSmsWebhookRequestConverter
      IWebSmsApiClient.cs                -- Aggregate client contract
    WebSmsNet/                         -- Core implementation (NuGet package)
      Connectors/                        -- Connector implementations (MessagingConnector)
      WebSmsApiClient.cs                 -- Aggregate client, wires connectors to the connection handler
      WebSmsApiConnectionHandler.cs      -- HTTP POST dispatcher with virtual extension points
    WebSmsNet.AspNetCore/              -- DI integration (NuGet package)
      Configuration/                     -- AddWebSmsApiClient(IServiceCollection, ...) extension
      Helpers/                           -- WebSmsWebhook parsing/matching helpers for webhook endpoints
  tests/
    WebSmsNet.Tests/                   -- xUnit tests (DI, serialization, webhook parse, live send when env set)
  build/
    GetBuildVersion.psm1               -- PowerShell version helper used by the release workflow
  .github/
    workflows/                         -- main.yml (build/publish), codeql-analysis.yml, sonar-analysis.yml
    dependabot.yml
```

### Dependency Graph

```
WebSmsNet.Tests --> WebSmsNet.AspNetCore --> WebSmsNet --> WebSmsNet.Abstractions
```

## Build Commands

```bash
# Restore, build, test the whole solution
dotnet restore WebSmsNet.sln
dotnet build   WebSmsNet.sln
dotnet build   WebSmsNet.sln -c Release       # also produces .nupkg (GeneratePackageOnBuild)
dotnet test    WebSmsNet.sln

# Run just the tests (env vars required for the live-send tests — see below)
dotnet test tests/WebSmsNet.Tests/WebSmsNet.Tests.csproj

# Pack explicitly
dotnet pack WebSmsNet.sln -c Release
```

### Env vars for live integration tests

The current `MessagingTests` fixture constructs a real `WebSmsApiClient` against `https://api.linkmobility.eu/` and reads credentials / recipient from the environment. Tests that hit the API will fail without these; pure serialization / parsing tests do not need them:

| Variable                        | Purpose                                           |
| ------------------------------- | ------------------------------------------------- |
| `Websms_AccessToken`            | Bearer token used by `SendTextMessage` / `SendBinaryMessage` |
| `Websms_RecipientAddressList`   | A test recipient MSISDN (E.164, single entry)     |

## Key Conventions

### Architecture Pattern
- **Aggregate client + connectors.** `IWebSmsApiClient` is the single entry point and exposes one property per API area. Today only `Messaging` (`IMessagingConnector`) exists; future endpoint groups add new connector properties alongside it.
- **Connection handler in the middle.** `WebSmsApiConnectionHandler` wraps a single `HttpClient`, owns JSON serialization, and is the only thing that talks HTTP. Connectors call `connectionHandler.Post<T>(endpoint, data, cancellationToken)` — they do not touch `HttpClient` directly.
- **Three construction paths, one handler contract.** `WebSmsApiClient` has constructors for `WebSmsApiOptions`, `HttpClient`, and `WebSmsApiConnectionHandler`. The DI path resolves the handler through `AddHttpClient<WebSmsApiConnectionHandler>` (typed client).
- **Extensible via virtuals.** `WebSmsApiConnectionHandler` exposes `SerializerOptions`, `OnBeforePost`, `OnResponseReceived`, `EnsureSuccess`, and virtual `Post<T>`. Consumers derive a custom handler to add auditing, retries, custom error mapping, etc. — see the README "Custom connection handler" section.
- **Endpoint path constants live next to the connector.** `MessagingConnector.MessagingApiBasePath = "/rest/smsmessaging"` — do not inline endpoint strings scattered across the codebase.

### Code Style
- **Sealed records for DTOs and responses.** `MessageSendResponse` is a `sealed record` with `init` properties and `required`. New response DTOs should follow the same shape.
- **Classes for request bodies.** `SmsSendRequest`, `TextSmsSendRequest`, `BinarySmsSendRequest` are classes today (with a mutable, settable base) — this preserves the existing public API. New mutable request shapes may stay as classes; new immutable ones should be records.
- **Nullable reference types enabled** (`<Nullable>enable</Nullable>`) in every csproj.
- **XML doc comments on every public / protected member.** All shipping projects set `<GenerateDocumentationFile>true</GenerateDocumentationFile>` — missing doc comments produce build warnings.
- **`[JsonPropertyName]` on every serialized property.** Names match the websms JSON exactly (camelCase, including quirks like `validityPeriode`).
- **`[SuppressMessage("ReSharper", "...")]`** is used liberally for R# noise on public API types (e.g., `UnusedMember.Global`, `ClassNeverInstantiated.Global`). Match the existing style.
- **`[Optional] CancellationToken`** is used across public async methods — keep the convention.

### Serialization
- `System.Text.Json` with `JsonSerializerDefaults.Web`, `DefaultIgnoreCondition = WhenWritingNull`, `WriteIndented = false`.
- Enums serialize as **camelCase strings** via `JsonStringEnumConverter(JsonNamingPolicy.CamelCase)` — except `WebSmsStatusCode`, which is serialized as its **integer value** via `[JsonConverter(typeof(JsonNumberEnumConverter<WebSmsStatusCode>))]` on each response property.
- Inbound webhook payloads discriminate on the `messageType` string (`"text"`, `"binary"`, `"deliveryReport"`). The `WebSmsWebhookRequestConverter` reads the discriminator and re-deserializes into the matching subtype of `WebSmsWebhookRequest.Base`.
- Both the converter and the enum converter are pre-registered in `WebSmsJsonSerialization.DefaultOptions`. Use these options everywhere (including webhook parsing) so inbound and outbound behavior stay consistent.

### Authentication
- Two modes, selected by `WebSmsApiOptions.AuthenticationType`:
  - `Bearer` — requires `AccessToken`, sent as `Authorization: Bearer <token>`.
  - `Basic` — requires `Username` + `Password`, sent as `Authorization: Basic <base64(user:pass)>`.
- The `HttpClient` is configured once by `HttpClientExtension.ApplyWebSmsApiOptions`. No per-request header mutation.
- Missing credentials throw `InvalidOperationException` at client construction / first use. There is no silent fallback.

### Webhook Handling
- Consumers receive three shapes from the websms platform: `WebSmsWebhookRequest.Text`, `.Binary`, `.DeliveryReport`, all deriving from `WebSmsWebhookRequest.Base`.
- Parse with `WebSmsWebhook.Parse(string json)` or `WebSmsWebhook.Parse(Stream requestStream, ...)`. Dispatch with `Match(onText, onBinary, onDeliveryReport)` — both an `Action` and a `Func<T>` overload exist.
- Respond with `WebSmsWebhook.CreateOkResponse()` / `CreateErrorResponse(string message)`. The helper uses `WebSmsStatusCode.Ok` / `InternalError` and a status message.
- Binary payloads are Base64-encoded segments optionally prefixed by a UDH; `BinaryContent.Parse` / `BinaryContent.CreateMessageContentParts` split and reassemble concatenated SMS segments.

### Testing Pattern (current state)
- **Framework:** xUnit 2.9 + FluentAssertions 6.12 + Coverlet. `[Fact]` for individual tests.
- **Locations:** a single test project (`tests/WebSmsNet.Tests`). Tests cover DI wiring, serialization/deserialization of `MessageSendResponse`, webhook parsing, `BinaryContent` round-trip, and live send (requires env vars).
- **Live-hit tests:** construct the client in a field initializer that reads `Websms_AccessToken` / `Websms_RecipientAddressList` from the environment and throws if absent. Do not hardcode credentials.
- **Assertions:** FluentAssertions (`result.Should().Be(...)`). Match existing style when adding tests — do not mix in NUnit or Shouldly.

## Important File Locations

| Purpose                        | Path                                                                       |
| ------------------------------ | -------------------------------------------------------------------------- |
| AI rules                       | `ai_instructions.md`                                                       |
| Main client interface          | `src/WebSmsNet.Abstractions/IWebSmsApiClient.cs`                           |
| Client implementation          | `src/WebSmsNet/WebSmsApiClient.cs`                                         |
| HTTP connection handler        | `src/WebSmsNet/WebSmsApiConnectionHandler.cs`                              |
| Messaging connector (interface)| `src/WebSmsNet.Abstractions/Connectors/IMessagingConnector.cs`             |
| Messaging connector (impl)     | `src/WebSmsNet/Connectors/MessagingConnector.cs`                           |
| API options                    | `src/WebSmsNet.Abstractions/Configuration/WebSmsApiOptions.cs`             |
| HttpClient configuration       | `src/WebSmsNet.Abstractions/Configuration/HttpClientExtension.cs`          |
| Authentication types           | `src/WebSmsNet.Abstractions/Configuration/AuthenticationType.cs`           |
| DI registration                | `src/WebSmsNet.AspNetCore/Configuration/WebSmsApiServiceCollectionExtension.cs` |
| Webhook helpers                | `src/WebSmsNet.AspNetCore/Helpers/WebSmsWebhook.cs`                        |
| Binary content helper          | `src/WebSmsNet.Abstractions/Helpers/BinaryContent.cs`                      |
| JSON defaults                  | `src/WebSmsNet.Abstractions/Serialization/WebSmsJsonSerialization.cs`      |
| Webhook polymorphic converter  | `src/WebSmsNet.Abstractions/Serialization/WebSmsWebhookRequestConverter.cs`|
| Request DTOs                   | `src/WebSmsNet.Abstractions/Models/*SmsSendRequest.cs`                     |
| Response DTOs                  | `src/WebSmsNet.Abstractions/Models/MessageSendResponse.cs`                 |
| Webhook DTOs                   | `src/WebSmsNet.Abstractions/Models/WebSmsWebhookRequest.cs`, `WebSmsWebhookResponse.cs` |
| Enums                          | `src/WebSmsNet.Abstractions/Models/Enums/`                                 |
| Tests                          | `tests/WebSmsNet.Tests/MessagingTests.cs`                                  |
| CI build & publish             | `.github/workflows/main.yml`                                               |
| CodeQL                         | `.github/workflows/codeql-analysis.yml`                                    |
| SonarCloud                     | `.github/workflows/sonar-analysis.yml`                                     |
| Release version helper         | `build/GetBuildVersion.psm1`                                               |

## Known Constraints and Gotchas

1. **Live tests need real credentials.** `MessagingTests.SendTextMessage` / `SendBinaryMessage` construct a `WebSmsApiClient` in a field initializer and will throw if `Websms_AccessToken` / `Websms_RecipientAddressList` are missing — the *whole test class* cannot instantiate. When running tests locally or in CI without credentials, expect these to fail at test discovery/setup. Parse / serialize / DI tests do not need env vars but live alongside the failing-constructor fixture.
2. **`WebSmsStatusCode` is an integer on the wire.** The enum's members use explicit websms codes (e.g., `Ok = 2000`). Serialize via `JsonNumberEnumConverter<WebSmsStatusCode>` — never as a camelCase string, or the API will reject the payload.
3. **Webhook discriminator is `messageType`.** `WebSmsWebhookRequestConverter` dispatches on `"text"`, `"binary"`, `"deliveryReport"`; any other value throws `JsonException`. If websms ever adds a new webhook type, extend the switch there and add a sibling type under `WebSmsWebhookRequest`.
4. **`WebSmsApiConnectionHandler` owns its `HttpClient` lifecycle only when constructed via `WebSmsApiOptions`.** In the DI path, `IHttpClientFactory` owns the socket. Do not dispose the handler-held client in derived classes.
5. **POST-only API surface.** The handler currently only exposes `Post<T>(...)`. If a future endpoint needs GET/PUT/DELETE, extend the handler with matching virtual methods — do not instantiate `HttpClient` from a connector.
6. **`GeneratePackageOnBuild` is on for all three library projects.** Every `dotnet build` produces `.nupkg` files in `bin/<Config>/`. `.gitignore` excludes `bin/` and `obj/`.
7. **Target framework is `net9.0`, LangVersion 13.** Do not mix `net10.0` / C# 14 language features into this repo without an explicit framework bump — other AMANDA-Technology libraries (BexioApiNet, CashCtrlApiNet) target `net10.0`, this one currently does not.
8. **`validityPeriode` (sic).** The property name on the wire is misspelled by the websms API. `SmsSendRequest.ValidityPeriod` maps to `[JsonPropertyName("validityPeriode")]` — keep the misspelling on the JSON side.
9. **No pagination, search, bulk, or binary-file endpoints.** Unlike BexioApiNet / CashCtrlApiNet, the websms Messaging API is a single POST-per-call surface. Do not import the `FetchAll` / `SearchCriteria` / `PostBulkAsync` abstractions from those libraries.

## Related AMANDA-Technology Repositories

These are sibling .NET API-client libraries by the same org and are useful as style references, though their tech stacks may differ (`net10.0`, NUnit + Shouldly, different API shape):

- [BexioApiNet](https://github.com/AMANDA-Technology/BexioApiNet) — Bexio REST API v3.0.0 (accounting/banking). Has both `CLAUDE.md` and `ai_instructions.md`; its connector + `ConnectionHandler` + DI layout directly inspired this one.
- [CashCtrlApiNet](https://github.com/AMANDA-Technology/CashCtrlApiNet) — CashCtrl REST API v1 (Swiss cloud ERP). Connector-per-domain pattern, form-encoded bodies, similar three-package split.

When taking inspiration from those repos, copy the *patterns* (connector/handler split, DI registration shape, XML doc discipline) rather than the *tech* (framework version, testing libraries) — this project is older and has its own stack as documented above.

## AI Agent Workflow (Quick Reference)

For full rules, see [`ai_instructions.md`](./ai_instructions.md). At a glance:

- **Personas:** documentation/onboarding → *developer* (C# library). Infrastructure-only issues (workflows, Dockerfiles, scripts) → *devops*. Architecture blueprints / cross-repo specs → *architect*.
- **Issue complexity:** `trivial` (one-file, one-line fix), `simple` (single feature / doc), `complex` (multi-file feature), `epic` (multi-phase initiative).
- **Scope discipline.** Make the minimum change that fulfills the task. Do not refactor unrelated code or migrate the test framework, target framework, or language version as a side effect.
- **Build before finishing.** Run `dotnet build` (0 errors, 0 warnings — XML doc warnings count) and `dotnet test` where feasible.
- **Commit before finishing.** Every agent run ends with either a clean tree or a new commit. Never leave staged/unstaged diffs behind.
- **Secrets.** Never write tokens, recipient MSISDNs, or customer data into source, tests, fixtures, or docs. Reference env vars by name only.
