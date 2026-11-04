# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build Facturama.slnx                                   # warnings are errors (analyzers: AnalysisLevel latest-recommended)
dotnet test Facturama.slnx --filter "Category!=Live"          # offline suite only (no Sandbox needed)
dotnet test Facturama.slnx --filter "Category=Live"           # Sandbox catalog tests (needs FACTURAMA_SANDBOX_USER / _PASSWORD)
dotnet test tests/Facturama.Tests --filter "FullyQualifiedName~CfdiUsesMatchTheApi"   # single test
dotnet format Facturama.slnx --verify-no-changes              # style check (runs in CI)
dotnet pack Facturama.slnx -c Release -o artifacts            # both NuGet packages
dotnet run --project examples/Facturama.Examples              # list the runnable examples
```

The SDK targets **.NET 10 (the current LTS)**; `global.json` pins the SDK (`rollForward: latestFeature`). `net10.0` is set once in `Directory.Build.props` together with `Nullable`, `TreatWarningsAsErrors` and the analyzers; package versions are central in `Directory.Packages.props`.

CI (`.github/workflows/ci.yml`) builds, runs the offline tests on Linux/Windows/macOS, `dotnet format --verify-no-changes` and `dotnet pack`. The `live` Sandbox job is separate and non-blocking, and only runs when the repo secrets `FACTURAMA_SANDBOX_USER`/`FACTURAMA_SANDBOX_PASSWORD` exist (the old public `pruebas` account now returns 401). Pushing a `v*` tag runs `.github/workflows/release.yml`, which requires the tag to equal `v` + the `<Version>` in `Directory.Build.props`, packs, optionally pushes to NuGet (secret `NUGET_API_KEY`) and creates the GitHub release.

## Architecture

This is a thin SDK for the Facturama (Mexican CFDI invoicing) REST API, ported from the PHP SDK (`Facturama/facturama-php-sdk`). Keep both in sync when the API changes. Everything lives in the `Facturama` namespace (`src/Facturama`).

- `FacturamaClient` is the whole HTTP layer. It exposes only `GetAsync/PostAsync/PutAsync/DeleteAsync(path, ...)` and returns the decoded JSON as `JsonNode?` (`null` for an empty body). There are **no per-resource models**: callers pass anonymous objects/records/dictionaries (serialized as written, the API is PascalCase) and API paths such as `catalogs/CfdiUses`. The `HttpClient` is injectable (second constructor argument); the client never mutates it (Basic auth and `User-Agent` are set per request) and only disposes the one it creates itself. That is how tests mock HTTP (`tests/Facturama.Tests/Support/StubHandler.cs`).
- `Resources/*` (`Cfdis`, `Clients`, `Products`, reached via `client.Cfdis` etc.) are thin wrappers that only build the HTTP calls; the endpoint paths mirror `examples/Facturama.Examples` and the PHP resources.
- `Exceptions/*`: `SendAsync` translates failures. An error status becomes `RequestException` (using the API's `Message`, `StatusCode` set; `ModelState` messages wrapped in a chained `ModelException`), network errors and timeouts become `RequestException` too (caller cancellation is **not** wrapped), and an unparseable success body raises `ResponseException`. All derive from `FacturamaException`. The namespace is `Facturama.Exceptions` (plural) because `Facturama.Exception` would shadow `System.Exception`.
- `src/Facturama.Extensions.DependencyInjection` is the optional integration (counterpart of the PHP Laravel provider): `AddFacturama(...)` registers a singleton `FacturamaClient` over a named `HttpClient` (`SocketsHttpHandler` with `PooledConnectionLifetime`, infinite handler lifetime because the client is a singleton). The core package must not reference `Microsoft.Extensions.*`.
- The default base URL is the **Sandbox** (`https://apisandbox.facturama.mx`); production needs `FacturamaEnvironment.Production` or `SetApiUrl(FacturamaClient.ProductionUrl)`.
- `Model/Catalog/*` are static offline copies of SAT/Facturama catalogs (`CfdiType`, `CfdiUse`, `PaymentForm`, `PaymentMethod`, `FederalTax`). Each is a `sealed class : ICatalog<TEntry>` with `public static readonly` entries (`CatalogEntry` / `CfdiTypeEntry` / `CfdiUseEntry` records), an `Entries` read-only list and `FindByName()`/`FindByValue()`/`FindAllByValue()`. When the API adds codes (e.g. CFDI 4.0 `S01`, `CP01`, `CN01`), add the field **and** append it to the `AllEntries` array. They were generated from the PHP catalogs; avoid member names that are type names (analyzer CA1720, e.g. `Single` → `SingleExhibition`).
- `examples/Facturama.Examples` is a console app with runnable examples (dispatcher in `Program.cs`) for the Web API, the Multi-issuer API and DI. Only CFDI 4.0 examples exist; 3.3 can no longer be stamped. Credentials come from `FACTURAMA_USER`/`FACTURAMA_PASSWORD`.

## Testing notes

- `ClientTests`, `ResourcesTests`, `DependencyInjectionTests` and `CatalogsOfflineTests` are offline (stub `HttpMessageHandler`, local catalogs). `CatalogsLiveTests` carry `[Trait("Category", "Live")]`, use `[LiveFact]` (skipped when the Sandbox credentials are not set) and compare the local catalogs against the **live Sandbox API**, so they can fail when the remote catalog changes. The "no unused entries" checks require the local catalog and the API to match exactly, so removing or renaming an entry can break them.
- xUnit v2 with the VSTest runner (`dotnet test`). Test projects relax `CS1591` only; analyzers still apply to them.

## Versioning

`<Version>` in `Directory.Build.props` is the single source of truth; `FacturamaClient.Version` (assembly informational version, build metadata stripped) and `UserAgent` are derived from it, and a test checks they match. Keep it in sync with `CHANGELOG.md` when releasing.

## Code style

File-scoped namespaces, nullable reference types and warnings-as-errors are enforced (`.editorconfig` + `dotnet format`). Public API needs XML docs (`CS1591` is a warning, and warnings are errors); the catalog files disable it with a pragma because there is one entry per code.

## Attribution

Wherever a pull request description, GitHub comment or generated document would end with a "Generated by Claude Code" / "Generated with Claude Code" line, write **"Generated by SkyNet"** instead.
