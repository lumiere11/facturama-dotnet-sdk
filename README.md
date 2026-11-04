[![CI](https://github.com/lumiere11/facturama-dotnet-sdk/actions/workflows/ci.yml/badge.svg)](https://github.com/lumiere11/facturama-dotnet-sdk/actions/workflows/ci.yml)

# Facturama SDK para .NET

> This document is also available in [English](README-en.md).
>
> Librería para consumir la API Web y la API Multiemisor de [Facturama](https://api.facturama.mx/).
>
> Puedes consultar la guía completa de la [API](https://apisandbox.facturama.mx/guias).

## Crear cuenta de usuario

> Crear una cuenta de usuario en el ambiente de prueba [Sandbox](https://dev.facturama.mx/api/login).
>
> Para API Web, realiza la configuración básica usando el RFC de pruebas **"EKU9003173C9"**, más información [aquí](https://apisandbox.facturama.mx/guias/perfil-fiscal).
>
> Sellos Digitales de prueba (CSD), [aquí](https://apisandbox.facturama.mx/guias/conocimientos/sellos-digitales-pruebas).

## Inicio rápido

### Dependencias

Esta librería requiere **.NET 10** (la versión LTS vigente, con soporte hasta noviembre de 2028). Sólo depende de la librería base de .NET: no trae paquetes de terceros.

### Instalación

```sh
dotnet add package Facturama.Sdk
```

Para la integración con inyección de dependencias (ASP.NET Core / Generic Host):

```sh
dotnet add package Facturama.Sdk.Extensions.DependencyInjection
```

### Crear una instancia de `FacturamaClient`

```csharp
using Facturama;

using var facturama = new FacturamaClient("USER", "PASSWORD");
```

> **Importante:** por defecto el cliente apunta al ambiente **Sandbox** (`https://apisandbox.facturama.mx`).
> Para timbrar en producción usa `new FacturamaClient(new FacturamaOptions { User = "USER", Password = "PASSWORD", Environment = FacturamaEnvironment.Production })`
> o `facturama.SetApiUrl(FacturamaClient.ProductionUrl)`.

Todas las operaciones son asíncronas (`Task`), aceptan un `CancellationToken` y devuelven el JSON de la respuesta como `JsonNode?` (`System.Text.Json.Nodes`), o `null` si la respuesta viene vacía. El cuerpo de las peticiones puede ser un objeto anónimo, un `record`, un diccionario o un `JsonNode`; los nombres de las propiedades se respetan tal cual (la API usa PascalCase).

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

## ASP.NET Core / inyección de dependencias

El paquete `Facturama.Sdk.Extensions.DependencyInjection` es el equivalente de la integración con Laravel de la versión PHP: registra un `FacturamaClient` compartido (singleton) configurado desde `IConfiguration`.

### 1. Configurar credenciales

`appsettings.json` (o *user secrets* / variables de entorno, **nunca** las subas al repositorio):

```json
{
  "Facturama": {
    "User": "tu-usuario",
    "Password": "tu-contraseña",
    "Environment": "Sandbox"
  }
}
```

| Clave | Descripción |
|---|---|
| `User` / `Password` | Credenciales de tu cuenta de Facturama. |
| `Environment` | `Sandbox` (por defecto, sólo datos de prueba) o `Production` (CFDIs reales). Cualquier otro valor lanza una excepción. |
| `ApiUrl` | Opcional. URL base explícita; tiene prioridad sobre `Environment`. |
| `ConnectTimeout` / `Timeout` | Opcionales. Por defecto `00:00:10` y `00:01:00`. |

> Usa `Sandbox` mientras desarrollas. Cambia a `Production` sólo cuando vayas a timbrar facturas reales.
> En producción define las variables como `Facturama__User`, `Facturama__Password` y `Facturama__Environment`.

### 2. Registrar el servicio

```csharp
using Facturama.Extensions.DependencyInjection;

builder.Services.AddFacturama(builder.Configuration.GetSection("Facturama"));
// o, en código:
builder.Services.AddFacturama(options =>
{
    options.User = builder.Configuration["Facturama:User"];
    options.Password = builder.Configuration["Facturama:Password"];
});
```

`AddFacturama` devuelve el `IHttpClientBuilder` del `HttpClient` subyacente (nombre `"Facturama"`), por lo que puedes añadirle *handlers* (logging, resiliencia, etc.).

### 3. Usar el cliente

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

### 4. Operaciones disponibles

| Recurso | Métodos |
|---|---|
| `Cfdis` | `CreateAsync(cfdi)`, `ListAsync(filters)`, `DownloadAsync(id, format, type)`, `SendByEmailAsync(id, email)`, `CancelAsync(id, motive)` |
| `Clients` | `ListAsync()`, `FindAsync(id)`, `CreateAsync(data)`, `UpdateAsync(id, data)`, `DeleteAsync(id)` |
| `Products` | `ListAsync()`, `FindAsync(id)`, `CreateAsync(data)`, `UpdateAsync(id, data)`, `DeleteAsync(id)` |

Para cualquier otro endpoint usa los métodos genéricos `GetAsync()`, `PostAsync()`, `PutAsync()` y `DeleteAsync()`, por ejemplo `facturama.GetAsync("catalogs/CfdiUses")`.

### 5. Manejo de errores

Todas las excepciones heredan de `Facturama.Exceptions.FacturamaException`:

| Excepción | Cuándo |
|---|---|
| `RequestException` | La API respondió con error (el mensaje es el `Message` de la API; `StatusCode` trae el código HTTP) o falló la llamada (red, *timeout*). Si la API devuelve `ModelState`, sus mensajes vienen encadenados en un `ModelException` como `InnerException`. |
| `ResponseException` | Una respuesta exitosa trae un cuerpo que no es JSON válido. |

Cancelar con el `CancellationToken` lanza `OperationCanceledException`, no `RequestException`.

### 6. Pruebas en tu aplicación

No llames a Facturama en tus pruebas: pasa un `HttpClient` con un `HttpMessageHandler` falso al constructor (`new FacturamaClient(httpClient)`), o registra tu propio handler:

```csharp
services.AddFacturama(o => o.User = "u").ConfigurePrimaryHttpMessageHandler(() => fakeHandler);
```

El proyecto de pruebas de este repositorio (`tests/Facturama.Tests/Support/StubHandler.cs`) es un ejemplo completo.

## Catálogos locales

`Facturama.Model.Catalog` incluye copias estáticas de los catálogos del SAT/Facturama: `CfdiType`, `CfdiUse`, `FederalTax`, `PaymentForm` y `PaymentMethod`. Cada uno expone `Entries`, `FindByName()`, `FindByValue()` y `FindAllByValue()`; `States` tiene los estados.

```csharp
using Facturama.Model.Catalog;

string uso = CfdiUse.GeneralExpenses.Value;           // "G03"
string metodo = PaymentMethod.FindByName("Pago en una sola exhibición").Value;  // "PUE"
bool soloFisicas = !CfdiUse.FindByValue("CN01").Moral;
```

## API Web

> Genera CFDIs usando un solo emisor.
>
> *Todas las operaciones se reflejan en la aplicación web de Facturama.*

### Operaciones

- Crear, consultar y cancelar CFDIs; descargar XML y PDF y enviarlos por correo;
- Consultar perfil y suscripción actual;
- Cargar logotipo y certificados digitales;
- CRUD de productos, clientes, sucursales y series.

## API Multiemisor

> Genera CFDIs usando múltiples emisores.
>
> *Estas operaciones NO se reflejan en la aplicación web de Facturama.*

### Operaciones

- Crear, consultar y cancelar CFDIs; descargar XML y PDF;
- CRUD de certificados de sellos digitales (CSD).

## Ejemplos

[`examples/Facturama.Examples`](examples/Facturama.Examples) es una aplicación de consola con ejemplos ejecutables de CFDI 4.0 (factura, público en general, factura global, cuenta de terceros, listar, descargar, enviar por correo y cancelar), CRUD de clientes y productos, API Multiemisor (CFDI y CSD) e inyección de dependencias. Usa tu cuenta del Sandbox:

```sh
export FACTURAMA_USER=tu-usuario
export FACTURAMA_PASSWORD=tu-contraseña
dotnet run --project examples/Facturama.Examples            # lista los ejemplos
dotnet run --project examples/Facturama.Examples -- cfdi-create
```

## Desarrollo

```sh
dotnet build Facturama.slnx
dotnet test Facturama.slnx --filter "Category!=Live"   # pruebas offline (sin red)
dotnet format Facturama.slnx --verify-no-changes       # estilo
```

Las pruebas `Category=Live` comparan los catálogos locales con el Sandbox y se omiten si no defines `FACTURAMA_SANDBOX_USER` y `FACTURAMA_SANDBOX_PASSWORD`.

## ¡Quiero contribuir!

Abre un *issue* o un *pull request*. Antes de enviarlo, asegúrate de que `dotnet build` (los *warnings* son errores), `dotnet test` y `dotnet format --verify-no-changes` pasan.

## Licencia

[MIT](LICENSE)
