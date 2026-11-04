[![CI](https://github.com/lumiere11/facturama-dotnet-sdk/actions/workflows/ci.yml/badge.svg)](https://github.com/lumiere11/facturama-dotnet-sdk/actions/workflows/ci.yml)

# Facturama SDK for .NET

> Este documento está disponible en [Español](README.md).
>
> Library to consume the Web API and Multi-issuer API of [Facturama](https://api.facturama.mx/).
>
> Check the Facturama guide [here](https://apisandbox.facturama.mx/guias).

## Create user account

> Create a user account in the [Sandbox](https://dev.facturama.mx/api/login) environment.
>
> For the Web API, use the test RFC **"EKU9003173C9"**, more information [here](https://apisandbox.facturama.mx/guias/perfil-fiscal).
>
> Test digital stamp certificates (CSDs), more information [here](https://apisandbox.facturama.mx/guias/conocimientos/sellos-digitales-pruebas).

## Quick start

### Requirements

This library targets **.NET 10**, the current LTS release (supported until November 2028). It has no third-party dependencies: only the .NET base class library.

### Install

```sh
dotnet add package Facturama.Sdk
```

For the dependency injection integration (ASP.NET Core / Generic Host):

```sh
dotnet add package Facturama.Sdk.Extensions.DependencyInjection
```

### Create a `FacturamaClient`

```csharp
using Facturama;

using var facturama = new FacturamaClient("USER", "PASSWORD");
```

> **Important:** by default the client targets the **Sandbox** environment (`https://apisandbox.facturama.mx`).
> To issue real CFDIs use `new FacturamaClient(new FacturamaOptions { User = "USER", Password = "PASSWORD", Environment = FacturamaEnvironment.Production })`
> or `facturama.SetApiUrl(FacturamaClient.ProductionUrl)`.

Every operation is asynchronous (`Task`), accepts a `CancellationToken` and returns the response JSON as a `JsonNode?` (`System.Text.Json.Nodes`), or `null` when the response is empty. Request bodies can be an anonymous object, a `record`, a dictionary or a `JsonNode`; property names are sent as written (the API uses PascalCase).

```csharp
var cfdi = await facturama.Cfdis.CreateAsync(new
{
    Currency = "MXN",
    ExpeditionPlace = "78140",
    CfdiType = "I",
    PaymentForm = "01",
    PaymentMethod = "PUE",
    Exportation = "01",
    Receiver = new { Rfc = "URE180429TM6", Name = "UNIVERSIDAD ROBOTICA ESPAÑOLA", CfdiUse = "G03", FiscalRegime = "603", TaxZipCode = "65000" },
    Items = new[]
    {
        new
        {
            ProductCode = "10101504", Description = "Estudios de viabilidad", Unit = "NO APLICA", UnitCode = "E48",
            UnitPrice = 50.0, Quantity = 2.0, Subtotal = 100.0, TaxObject = "02", Total = 116.0,
            Taxes = new[] { new { Total = 16.0, Name = "IVA", Base = 100.0, Rate = 0.16, IsRetention = false } },
        },
    },
});

Console.WriteLine(cfdi?["Id"]);
```

## ASP.NET Core / dependency injection

The `Facturama.Sdk.Extensions.DependencyInjection` package is the counterpart of the PHP SDK's Laravel integration: it registers one shared (singleton) `FacturamaClient` configured from `IConfiguration`.

### 1. Configure credentials

`appsettings.json` (or user secrets / environment variables; **never** commit credentials):

```json
{
  "Facturama": {
    "User": "your-user",
    "Password": "your-password",
    "Environment": "Sandbox"
  }
}
```

| Key | Description |
|---|---|
| `User` / `Password` | Your Facturama account credentials. |
| `Environment` | `Sandbox` (default, test data only) or `Production` (real CFDIs). Any other value throws an exception. |
| `ApiUrl` | Optional. Explicit base URL; takes precedence over `Environment`. |
| `ConnectTimeout` / `Timeout` | Optional. Default `00:00:10` and `00:01:00`. |

> Use `Sandbox` while developing. Switch to `Production` only when you are going to stamp real invoices.
> In production set the variables as `Facturama__User`, `Facturama__Password` and `Facturama__Environment`.

### 2. Register the service

```csharp
using Facturama.Extensions.DependencyInjection;

builder.Services.AddFacturama(builder.Configuration.GetSection("Facturama"));
// or in code:
builder.Services.AddFacturama(options =>
{
    options.User = builder.Configuration["Facturama:User"];
    options.Password = builder.Configuration["Facturama:Password"];
});
```

`AddFacturama` returns the `IHttpClientBuilder` of the underlying `HttpClient` (named `"Facturama"`), so you can add handlers (logging, resilience, ...).

### 3. Use the client

```csharp
app.MapPost("/invoices", async (InvoiceRequest request, FacturamaClient facturama, CancellationToken ct) =>
{
    try
    {
        var cfdi = await facturama.Cfdis.CreateAsync(request.ToCfdi(), ct);

        return Results.Created($"/invoices/{cfdi?["Id"]}", cfdi);
    }
    catch (RequestException e)
    {
        return Results.UnprocessableEntity(new { error = e.Message });
    }
});

app.MapGet("/invoices/{id}/pdf", async (string id, FacturamaClient facturama, CancellationToken ct) =>
{
    var file = await facturama.Cfdis.DownloadAsync(id, FileTypes.Pdf, cancellationToken: ct);

    return Results.File(Convert.FromBase64String(file!["Content"]!.GetValue<string>()), "application/pdf");
});
```

### 4. Available operations

| Resource | Methods |
|---|---|
| `Cfdis` | `CreateAsync(cfdi)`, `ListAsync(filters)`, `DownloadAsync(id, format, type)`, `SendByEmailAsync(id, email)`, `CancelAsync(id, motive)` |
| `Clients` | `ListAsync()`, `FindAsync(id)`, `CreateAsync(data)`, `UpdateAsync(id, data)`, `DeleteAsync(id)` |
| `Products` | `ListAsync()`, `FindAsync(id)`, `CreateAsync(data)`, `UpdateAsync(id, data)`, `DeleteAsync(id)` |

For any other endpoint use the generic `GetAsync()`, `PostAsync()`, `PutAsync()` and `DeleteAsync()` methods, e.g. `facturama.GetAsync("catalogs/CfdiUses")`.

### 5. Error handling

Every exception derives from `Facturama.Exceptions.FacturamaException`:

| Exception | When |
|---|---|
| `RequestException` | The API answered with an error (the message is the API's `Message`; `StatusCode` carries the HTTP status) or the call itself failed (network, timeout). When the API returns `ModelState`, its messages are chained as a `ModelException` inner exception. |
| `ResponseException` | A successful response carries a body that is not valid JSON. |

Cancelling through the `CancellationToken` throws `OperationCanceledException`, not `RequestException`.

### 6. Testing your application

Do not call Facturama in your tests: pass an `HttpClient` with a fake `HttpMessageHandler` to the constructor (`new FacturamaClient(httpClient)`), or register your own handler:

```csharp
services.AddFacturama(o => o.User = "u").ConfigurePrimaryHttpMessageHandler(() => fakeHandler);
```

This repository's test project (`tests/Facturama.Tests/Support/StubHandler.cs`) is a complete example.

## Local catalogs

`Facturama.Model.Catalog` ships static copies of the SAT/Facturama catalogs: `CfdiType`, `CfdiUse`, `FederalTax`, `PaymentForm` and `PaymentMethod`. Each exposes `Entries`, `FindByName()`, `FindByValue()` and `FindAllByValue()`; `States` holds the Mexican states.

```csharp
using Facturama.Model.Catalog;

string use = CfdiUse.GeneralExpenses.Value;                                  // "G03"
string method = PaymentMethod.FindByName("Pago en una sola exhibición").Value; // "PUE"
bool naturalOnly = !CfdiUse.FindByValue("CN01").Moral;
```

## Web API

> Make CFDIs by using one issuer.
>
> *All operations will be reflected on Facturama's web app.*

### Operations

- Create, get, cancel CFDIs; download XMLs and PDFs and send them by email;
- Check profile and current subscription;
- Logo and digital certificates uploading;
- CRUDs for Product, Customer, Branch office and series.

## Multi-issuer API

> Make CFDIs by using multiple issuers.
>
> *These operations will NOT be reflected on Facturama's web app.*

### Operations

- Create, get, cancel CFDIs; download XMLs and PDFs;
- CRUD for digital sign certificates ("CSD", "Certificados de los Sellos Digitales").

## Examples

[`examples/Facturama.Examples`](examples/Facturama.Examples) is a console app with runnable CFDI 4.0 examples (invoice, public-in-general, global invoice, third-party account, list, download, email and cancel), client and product CRUD, the Multi-issuer API (CFDI and CSD) and dependency injection. Use your Sandbox account:

```sh
export FACTURAMA_USER=your-user
export FACTURAMA_PASSWORD=your-password
dotnet run --project examples/Facturama.Examples            # lists the examples
dotnet run --project examples/Facturama.Examples -- cfdi-create
```

## Development

```sh
dotnet build Facturama.slnx
dotnet test Facturama.slnx --filter "Category!=Live"   # offline tests (no network)
dotnet format Facturama.slnx --verify-no-changes       # style
```

`Category=Live` tests compare the local catalogs with the Sandbox and are skipped unless `FACTURAMA_SANDBOX_USER` and `FACTURAMA_SANDBOX_PASSWORD` are set.

## Contributing

Open an issue or a pull request. Before sending it, make sure `dotnet build` (warnings are errors), `dotnet test` and `dotnet format --verify-no-changes` pass.

## License

[MIT](LICENSE)
