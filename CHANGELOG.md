# Changelog

## 1.1.0

### Added
- Web API resources: `client.BranchOffices` (CRUD), `client.Series` (series of a branch office), `client.TaxEntity` (fiscal profile, `UploadCsdAsync`/`UploadCsdFromFilesAsync`, `UploadFielAsync`, `UploadLogoAsync`) and `client.Retentions` (withholding CFDIs: `CreateAsync`, `FindAsync`, `CancelAsync`).
- `Cfdis.FindAsync()` (one CFDI) and `Cfdis.StatusAsync()` (the status of a CFDI at the SAT).
- Offline SAT catalogs `FiscalRegime`, `CancellationMotive`, `TaxObject`, `Exportation` and `Periodicity`.
- The GitHub release of a version is described by its section of this changelog; the NuGet packages link to it (`PackageReleaseNotes`) and publish their repository for Source Link.
- Multi-issuer API resources: `client.Multiissuer.Cfdis` (`CreateAsync`, `FindAsync`, `ListAsync`, `DownloadAsync`, `SendByEmailAsync`, `CancelAsync`) and `client.Multiissuer.Csds` (`ListAsync`, `FindAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, plus `CreateFromFilesAsync`/`UpdateFromFilesAsync`, which read and base64-encode the `.cer` and `.key` files).
- `DownloadBytesAsync()` and `DownloadToFileAsync()` on `Cfdis` and `Multiissuer.Cfdis`: the decoded file instead of the JSON with the base64 `Content`. A response without a valid `Content` raises `ResponseException`.
- `ReceiptTypes.IssuedLite` (`issuedLite`), the type of the CFDIs issued through the Multi-issuer API.

## 1.0.0

First release of the .NET SDK: a port of the [Facturama PHP SDK](https://github.com/Facturama/facturama-php-sdk) 3.0.0 to **.NET 10 (LTS)**.

### Added
- `FacturamaClient`: the whole HTTP layer (`GetAsync`/`PostAsync`/`PutAsync`/`DeleteAsync`) over `HttpClient`, returning the decoded JSON as `JsonNode?`. Targets the Sandbox by default; `FacturamaOptions`/`FacturamaEnvironment`, `SetApiUrl()`, `IsProduction`, `SandboxUrl`/`ProductionUrl`. The `HttpClient` is injectable and never mutated (credentials and `User-Agent` are set per request).
- Typed resources: `Cfdis`, `Clients`, `Products` (`client.Cfdis`, ...), mirroring the PHP endpoints.
- Exceptions: `FacturamaException` base class, `RequestException` (API `Message`, `StatusCode`, validation messages chained as `ModelException`), `ResponseException` (unparseable success body).
- Offline catalogs `CfdiType`, `CfdiUse`, `FederalTax`, `PaymentForm`, `PaymentMethod` (typed `CatalogEntry` records, `ICatalog<T>` with static abstract members) and `States`, `FileTypes`, `ReceiptTypes`.
- `Facturama.Sdk.Extensions.DependencyInjection`: `AddFacturama(...)` registers a shared client from `IConfiguration` (the counterpart of the PHP Laravel integration).
- Runnable CFDI 4.0 examples (`examples/Facturama.Examples`), offline test suite and Sandbox `Category=Live` tests, GitHub Actions CI and tag-triggered release workflow.

### Differences from the PHP SDK
- Every operation is `async` and takes a `CancellationToken`; the exceptions live in `Facturama.Exceptions` (not `Facturama.Exception`, which would clash with `System.Exception`).
- All exceptions derive from `FacturamaException` (in the PHP code only the base class does; `RequestException` and `ResponseException` extend `RuntimeException`).
- `findByValue()` is uniform across catalogs: `FindByValue()` returns the first match or throws `ArgumentException`, and the new `FindAllByValue()` returns every match (PHP's `CfdiType::findByValue()` was a generator).
- `P01` (`CfdiUse.ToBeDefined`) is `[Obsolete]`: it no longer exists in CFDI 4.0.
- Resource ids are URL-escaped and a leading `/` in a path is ignored.
