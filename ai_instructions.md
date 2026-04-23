---
title: AI Agent Instructions for WebSmsNet
tags: [ai, instructions, contributing, conventions]
---

# AI Agent Instructions — WebSmsNet

This file contains **strict, procedural instructions** for AI coding agents working in this repository. Follow these rules to the letter — they keep the library production-grade and aligned with the LINK Mobility websms Messaging REST API.

- This file: **strict rules for AI agents** (what to do, what never to do).
- [`CLAUDE.md`](./CLAUDE.md): **high-level overview** (tech stack, structure, file locations, gotchas).
- [`README.md`](./README.md): **end-user documentation** (installation, usage, custom handler).

If anything here conflicts with `CLAUDE.md`, **this file wins for agent behavior**.

## 1. Mission & Source of Truth

1. This library is a typed .NET client for the **LINK Mobility websms Messaging REST API 1.0.0**. Source of truth: <https://developer.linkmobility.eu/sms-api/rest-api>.
2. Every endpoint, DTO field, enum value, status code and query / body parameter must match the websms docs exactly. If the docs and the code disagree, **the docs are right** — open a change.
3. Never invent endpoints, fields or behavior that the websms docs do not describe. If the docs are ambiguous, stop and ask rather than guessing.
4. The API's JSON is snake-ish camelCase with a handful of quirks (e.g., `validityPeriode` is misspelled on the wire). Preserve those on the JSON side via `[JsonPropertyName]`; the C# name should be the correctly-spelled `ValidityPeriod`.

## 2. Tech Stack (non-negotiable)

| Concern        | Value                                                              |
| -------------- | ------------------------------------------------------------------ |
| Language       | C# 13 (`<LangVersion>13</LangVersion>`)                            |
| Framework      | .NET 9 (`net9.0`) for libraries; .NET 10 (`net10.0`) for test projects |
| JSON           | `System.Text.Json` — **never** add Newtonsoft.Json                 |
| DI             | `Microsoft.Extensions.DependencyInjection` + `Microsoft.Extensions.Http` (typed clients) |
| Tests          | NUnit + Shouldly + NSubstitute + Bogus + coverlet.collector        |
| Nullability    | `<Nullable>enable</Nullable>` everywhere                           |
| XML docs       | `<GenerateDocumentationFile>true</GenerateDocumentationFile>` on all three shipping projects |
| Packaging      | `<GeneratePackageOnBuild>true</GeneratePackageOnBuild>` — build produces `.nupkg` for each library |

If a change would require bumping any of these (e.g., moving libraries to `net10.0` or swapping NUnit for another framework), **stop and escalate**. This project intentionally lags its siblings (BexioApiNet, CashCtrlApiNet) and must not be migrated silently.

## 3. Architecture Patterns (do not deviate)

### 3.1 Aggregate client + connectors
- `IWebSmsApiClient` (in `WebSmsNet.Abstractions`) is the aggregate root. It exposes one property per API area.
- Today the only area is `Messaging` (`IMessagingConnector`). Adding a new API area means:
  1. Add a new connector interface under `src/WebSmsNet.Abstractions/Connectors/I<Area>Connector.cs`.
  2. Add the implementation under `src/WebSmsNet/Connectors/<Area>Connector.cs`, constructor-injecting `WebSmsApiConnectionHandler`.
  3. Add the property to `IWebSmsApiClient` and assign it in `WebSmsApiClient`.
  4. No new DI registration is needed — connectors are wired directly by `WebSmsApiClient`. `AddWebSmsApiClient` only registers the handler and the client.

### 3.2 Single `WebSmsApiConnectionHandler` for HTTP
- All HTTP access goes through `WebSmsApiConnectionHandler`. Connectors **never** new up `HttpClient` or call it directly.
- The handler today exposes one public verb, `Task<T> Post<T>(string endpoint, object data, CancellationToken)`. Endpoints that need other verbs must be added as sibling virtual methods on the handler (e.g., `Get<T>`, `Delete`, `GetBinary`) — not inline in connectors.
- The handler is designed to be subclassed. The `SerializerOptions`, `OnBeforePost`, `OnResponseReceived`, `EnsureSuccess`, and `Post<T>` members are all `virtual` / `protected virtual`. Do not tighten these to `private` / `sealed` — the README documents deriving from the handler as the public extension point.
- Three construction paths exist, all ending in the same handler:
  - `WebSmsApiClient(WebSmsApiOptions)` — owns a new `HttpClient` (non-DI).
  - `WebSmsApiClient(HttpClient)` — caller supplies the client.
  - `WebSmsApiClient(WebSmsApiConnectionHandler)` — caller supplies a fully-custom handler.
  Keep all three working when touching the client.

### 3.3 DI registration
- `AddWebSmsApiClient(IServiceCollection, Action<WebSmsApiOptions>)`:
  1. `services.Configure(configureOptions)`.
  2. `services.AddHttpClient<WebSmsApiConnectionHandler>(...)` — typed client; reads `IOptions<WebSmsApiOptions>` inside the configure delegate and calls `ApplyWebSmsApiOptions`.
  3. `services.AddScoped<IWebSmsApiClient>(...)` — resolves `WebSmsApiConnectionHandler` from the provider and constructs `WebSmsApiClient`.
- Do **not** replace this with `services.AddScoped<WebSmsApiConnectionHandler>(...)` — that defeats `IHttpClientFactory` and brings back the classic socket-exhaustion bug.
- Do **not** change the scope from `Scoped` to `Singleton` without verifying `HttpClient` lifetime semantics — typed clients are scoped by default.

### 3.4 Endpoint paths as constants
- Paths live in a `private const string` on the connector (e.g., `MessagingConnector.MessagingApiBasePath = "/rest/smsmessaging"`). Endpoint *variants* (`"/text"`, `"/binary"`) are concatenated at call time.
- Do not inline literal path strings outside the connector. If a group of related endpoints grows beyond a few, promote the paths to a static sibling class (mirroring the `*Configuration` / `Endpoints` patterns in BexioApiNet / CashCtrlApiNet).

### 3.5 Request and response shapes
- **Request bodies** inherit from `SmsSendRequest` (an abstract class with `required` properties + `get; set;`). `TextSmsSendRequest` and `BinarySmsSendRequest` are `sealed`. Keep the abstract base mutable — consumers compose requests property-by-property.
- **Response bodies** are `sealed record` with `required ... { get; init; }`. Match this for any new response DTO.
- Every serialized property has `[JsonPropertyName("...")]` matching the websms wire name exactly (camelCase, including quirks). Do not rely on `JsonSerializerDefaults.Web` name inference.
- Nullable → nullable. If websms can return / accept `null`, the property type is nullable (`string?`, `int?`, `ContentCategory?`).

### 3.6 Enums
- Live in `src/WebSmsNet.Abstractions/Models/Enums/`. Each value has an XML `<summary>` with the human-readable meaning.
- Default enum serialization is **camelCase string** via the global `JsonStringEnumConverter(JsonNamingPolicy.CamelCase)` in `WebSmsJsonSerialization.DefaultOptions`.
- **Exception:** `WebSmsStatusCode` serializes as its **integer** websms code (e.g., `2000`). It is the only enum whose on-the-wire form is numeric. Apply this on each property that uses it:
  ```csharp
  [JsonPropertyName("statusCode")]
  [JsonConverter(typeof(JsonNumberEnumConverter<WebSmsStatusCode>))]
  public required WebSmsStatusCode StatusCode { get; init; }
  ```
  When adding a new response type that includes a status code, repeat this attribute pair. Do not "simplify" by removing either attribute.

### 3.7 Webhook polymorphism
- `WebSmsWebhookRequest` is a **static container class** with nested `Base`, `TextAndBinaryBase`, `Text`, `Binary`, `DeliveryReport`. The polymorphism is read through `WebSmsWebhookRequestConverter`, which dispatches on `messageType`.
- Adding a new webhook kind means:
  1. Add a nested class under `WebSmsWebhookRequest` inheriting from `Base` (or `TextAndBinaryBase`).
  2. Add a case to `WebSmsWebhookRequestConverter.Read` for the new discriminator value.
  3. Add a value to the `WebhookMessageType` enum.
  4. Extend `WebSmsWebhook.Match(...)` with an additional `onXxx` parameter — **breaking API change**, call it out.
- Inbound webhook parsing always uses `WebSmsJsonSerialization.DefaultOptions`. Do not parse webhook JSON with ad-hoc options.

### 3.8 Extension points on the handler
- `OnBeforePost(endpoint, data, cancellationToken)` — auditing / logging / pre-flight mutation.
- `OnResponseReceived(response, cancellationToken)` — runs **before** `EnsureSuccess`; returns the response (possibly replaced) for chaining.
- `EnsureSuccess(response)` — defaults to `response.EnsureSuccessStatusCode()`. Override to translate websms error bodies into richer exceptions.
- `SerializerOptions` — defaults to `WebSmsJsonSerialization.DefaultOptions`. Override to add custom converters or change casing.
- When extending the handler, **preserve this order**: `OnBeforePost` → HTTP call → `OnResponseReceived` → `EnsureSuccess` → `ReadFromJsonAsync`. Tests and consumers rely on it.

## 4. Model & DTO Rules

1. **Records for responses**, **classes for mutable requests** — match what exists today. Do not silently convert a class to a record (it changes equality and breaks consumers).
2. Use `required` for properties that websms always sends back or always needs. Use `init` for immutable properties on records; `get; set;` on request classes.
3. **Enable nullable reference types.** If websms can return `null`, the property type must be nullable.
4. Map property names with `[JsonPropertyName("...")]` that match websms exactly, including any misspellings on the websms side.
5. Place new DTOs under `src/WebSmsNet.Abstractions/Models/<Area>/` when an area grows beyond the current flat layout. Enums go in `Models/Enums/`.
6. Every `public` / `protected` type and member needs an XML `<summary>`. Missing docs produce build warnings, which we treat as errors.
7. `[SuppressMessage("ReSharper", "...")]` is used liberally on public API surface types to silence ReSharper's "unused" noise. Match the existing pattern rather than disabling R# project-wide.

## 5. Serialization Rules

1. Go through `WebSmsJsonSerialization.DefaultOptions` for all serialization / deserialization — outbound bodies via the handler, inbound webhooks via `WebSmsWebhook.Parse`.
2. Do not mutate `DefaultOptions` at call sites except for local diagnostic formatting (tests may set `WriteIndented = true` on a local copy).
3. If a new converter is needed, add it to the `Converters` collection in `WebSmsJsonSerialization.DefaultOptions` and keep a sibling `JsonConverter<T>` class in `src/WebSmsNet.Abstractions/Serialization/`.
4. Never write a Newtonsoft.Json converter. Never pull `Newtonsoft.Json` into the solution.

## 6. Testing Rules

1. **Framework:** NUnit 4.x + Shouldly 4.x + NSubstitute 5.x + Bogus + coverlet.collector. Use `[Test]` for individual tests and `[TestFixture]` for classes.
2. **Projects:** Tests are split into `WebSmsNet.UnitTests`, `WebSmsNet.IntegrationTests`, and `WebSmsNet.E2eTests`. Use the appropriate project for new tests.
3. **New connector methods require a test.** At minimum: a DI-wiring test verifying the method is reachable through `IWebSmsApiClient`, and a serialization round-trip for the request / response DTOs involved. Core library components (serialization, HTTP client extensions, binary content helpers) have full unit test coverage that must be maintained.
4. **Live API tests** use env vars (`Websms_AccessToken`, `Websms_RecipientAddressList`) and **must skip** (using `Assume.That`) when the vars are absent.
5. **Mocking:** Use NSubstitute for mocking dependencies. For isolation, you can also construct a fake `WebSmsApiConnectionHandler` subclass in the test project.
6. Do **not** add test-only code paths to production types. If a test needs an override, derive from the handler in the test project.
7. Never commit real tokens, recipient MSISDNs, or customer data in test fixtures. Read them from environment variables.

## 7. Build & Verification Rules

1. Before completing any task, run `dotnet build WebSmsNet.sln` and confirm **0 errors and 0 warnings**. XML-doc warnings count as errors for this project — all three shipping projects have `GenerateDocumentationFile` on.
2. Run `dotnet test` where feasible. The live-send fixture will fail without env vars — that is expected locally. Verify the parse/serialize/DI tests still pass.
3. Never introduce `.Result` or `.Wait()` on tasks. Use `async` / `await` end-to-end. The handler's `Post<T>` is already fully async.
4. Keep `ImplicitUsings` and `Nullable` enabled in every csproj. Do not disable them per-file.
5. Do not commit artifacts (`bin/`, `obj/`, `*.nupkg`). They are in `.gitignore`.

## 8. Secrets & Security

1. Never write real websms tokens, basic-auth passwords, customer MSISDNs, or webhook bodies containing real phone numbers into source, tests, fixtures, or docs.
2. `Websms_AccessToken` and `Websms_RecipientAddressList` are supplied via the environment only. Reference them by name — never by value.
3. Never log or serialize `Authorization` headers, `AccessToken`, or `Password` values. If adding logging in a custom handler, redact these fields.
4. Do not commit `.env`, `appsettings.Development.json`, or any file containing credentials.

## 9. Workflow Expectations for AI Agents

1. **Scope discipline.** Apply the minimum change that fulfills the task. Do not refactor unrelated code "on the way through".
2. **No speculative features.** Implement only what the issue / blueprint requests. Hypothetical future needs are not a reason to add abstractions.
3. **No silent framework / library migrations.** Target framework, C# language version, and testing stack are all explicit in this document. Changing any of them is its own ticket.
4. **Incremental verification.** Build after each significant change, not only at the end.
5. **Commit before finishing.** Every agent run ends with either a clean tree or a new commit — never staged/unstaged diffs.
6. **Update docs alongside code.** If a convention changes, update this file, `CLAUDE.md`, and `README.md` (if user-facing) in the same change.
7. If the websms docs describe something this library does not yet support, do not silently stub it. Either implement it end-to-end or open a tracked backlog item.

## 10. Forbidden Patterns

Do not:
- Instantiate `HttpClient` directly in new code. Always route through the injected / constructed `WebSmsApiConnectionHandler`.
- Throw bare exceptions from connector methods for ordinary API failures — rely on `EnsureSuccess` (and its override points) so consumers can plug in custom error translation.
- Convert `sealed record` response types to `class` or vice-versa without explicit need — both changes are breaking.
- Add Newtonsoft.Json, AutoMapper, MediatR, xUnit, FluentAssertions, Moq, or any framework not already on the dependency list.
- Skip XML doc comments on public members — they are compiled into the NuGet packages and missing docs break the build.
- Hardcode endpoint paths in multiple places — use the `const string` pattern on the connector.
- Silently change the `WebSmsStatusCode` serialization from integer to string, or drop `[JsonNumberEnumConverter<WebSmsStatusCode>]` from a response property.
- Bump `<TargetFramework>` from `net9.0` to `net10.0` or `<LangVersion>` beyond 13 as a side-effect. That is an explicit, standalone change.
- Rename `ValidityPeriod` → `ValidityPeriode` to "fix" the C# property name. The correctly-spelled C# name with the wire-misspelled `[JsonPropertyName]` is intentional.

## 11. Persona Routing

When an issue arrives, choose the persona that matches the work:

- **developer** — C# / .NET implementation, bug fixes, new connector methods, serialization work, test authoring. This is the default for anything under `src/` or `tests/`.
- **devops** — `.github/workflows/*.yml`, `build/*.psm1`, Dockerfiles, dependabot config, release packaging changes. Not subject to TDD.
- **architect** — cross-repo blueprints, large feature specs, breaking-API decisions. Produces design docs, not code.
- **verification** — reviews a feature branch's diff + build + tests before merge. Does not author feature code.

Onboarding / documentation tasks (this issue) are handled by *developer* with a docs-only diff.

## 12. When in Doubt

1. Re-read the relevant section of <https://developer.linkmobility.eu/sms-api/rest-api>.
2. Compare with the fully-implemented example in this repo: `MessagingConnector` + `TextSmsSendRequest` + `MessageSendResponse` + `MessagingTests` is the canonical reference.
3. Cross-check against the sibling libraries for *patterns only* — do not copy their tech choices:
   - [BexioApiNet](https://github.com/AMANDA-Technology/BexioApiNet) — connector + handler + DI patterns, `ai_instructions.md` template.
   - [CashCtrlApiNet](https://github.com/AMANDA-Technology/CashCtrlApiNet) — three-package split, connector-per-domain shape.
4. If still unclear, stop and surface the question in the task result rather than guessing.
