using System.Text.Json;
using System.Text.Json.Nodes;
using Facturama;
using Facturama.Examples;
using Facturama.Examples.Multiissuer;
using Facturama.Examples.Web;
using Facturama.Exceptions;

// Runnable examples of the Facturama SDK (CFDI 4.0). The client targets the Sandbox, so no real CFDI is stamped.
//
//   export FACTURAMA_USER=your-sandbox-user
//   export FACTURAMA_PASSWORD=your-sandbox-password
//   dotnet run --project examples/Facturama.Examples -- <example> [args]
var examples = new SortedDictionary<string, (string Description, Func<FacturamaClient, string[], Task<JsonNode?>> Run)>(StringComparer.Ordinal)
{
    // Web API (single issuer)
    ["cfdi-create"] = ("Stamp a CFDI 4.0 invoice", (c, _) => CfdiExamples.CreateInvoiceAsync(c)),
    ["cfdi-public"] = ("Stamp an invoice to Publico en General", (c, _) => CfdiExamples.CreatePublicInvoiceAsync(c)),
    ["cfdi-global"] = ("Stamp a global invoice (Factura Global)", (c, _) => CfdiExamples.CreateGlobalInvoiceAsync(c)),
    ["cfdi-third-party"] = ("Stamp an invoice with a third-party account", (c, _) => CfdiExamples.CreateThirdPartyInvoiceAsync(c)),
    ["cfdi-list"] = ("List the issued CFDIs", (c, a) => CfdiExamples.ListAsync(c, Arg(a, 0, "0"))),
    ["cfdi-download"] = ("Download a CFDI: <id> [pdf|xml|html]", (c, a) => CfdiExamples.DownloadAsync(c, Arg(a, 0), Arg(a, 1, FileTypes.Pdf))),
    ["cfdi-email"] = ("Send a CFDI by email: <id> <email>", (c, a) => CfdiExamples.SendByEmailAsync(c, Arg(a, 0), Arg(a, 1))),
    ["cfdi-cancel"] = ("Cancel a CFDI: <id> [motive]", (c, a) => CfdiExamples.CancelAsync(c, Arg(a, 0), Arg(a, 1, "02"))),
    ["client-crud"] = ("Create, read, update and delete a client", (c, _) => ClientExamples.RunAsync(c)),
    ["product-crud"] = ("Create, read, update and delete a product", (c, _) => ProductExamples.RunAsync(c)),
    ["branch-list"] = ("List the branch offices", (c, _) => c.GetAsync("api/BranchOffice")),

    // Multi-issuer API
    ["multi-cfdi-create"] = ("Stamp a CFDI 4.0 as one of many issuers", (c, _) => MultiissuerExamples.CreateCfdiAsync(c)),
    ["multi-cfdi-get"] = ("Get a CFDI: <id>", (c, a) => c.GetAsync($"api-lite/cfdis/{Uri.EscapeDataString(Arg(a, 0))}")),
    ["multi-cfdi-download"] = ("Download a CFDI: <id> [pdf|xml|html]", (c, a) => MultiissuerExamples.DownloadAsync(c, Arg(a, 0), Arg(a, 1, FileTypes.Pdf))),
    ["multi-cfdi-cancel"] = ("Cancel a CFDI: <id> [motive]", (c, a) => c.DeleteAsync($"api-lite/cfdis/{Uri.EscapeDataString(Arg(a, 0))}", new Dictionary<string, object?> { ["type"] = "issuedLite", ["motive"] = Arg(a, 1, "02") })),
    ["multi-csd-list"] = ("List the uploaded CSDs", (c, _) => c.GetAsync("api-lite/csds")),
    ["multi-csd-get"] = ("Get the CSD of an RFC: <rfc>", (c, a) => c.GetAsync($"api-lite/csds/{Uri.EscapeDataString(Arg(a, 0))}")),
    ["multi-csd-upload"] = ("Upload a CSD: <rfc> <cer-file> <key-file> <key-password>", (c, a) => MultiissuerExamples.UploadCsdAsync(c, Arg(a, 0), Arg(a, 1), Arg(a, 2), Arg(a, 3))),
    ["multi-csd-delete"] = ("Delete the CSD of an RFC: <rfc>", (c, a) => c.DeleteAsync($"api-lite/csds/{Uri.EscapeDataString(Arg(a, 0))}")),

    // Dependency injection
    ["di"] = ("Resolve the client from the DI container", (_, _) => DependencyInjectionExample.RunAsync()),
};

if (args.Length == 0 || !examples.TryGetValue(args[0], out var example))
{
    Console.Error.WriteLine("Usage: dotnet run --project examples/Facturama.Examples -- <example> [args]");
    Console.Error.WriteLine();
    foreach ((string name, var info) in examples)
    {
        Console.Error.WriteLine($"  {name,-20} {info.Description}");
    }

    return args.Length == 0 ? 0 : 1;
}

// A Sandbox account is required; the old public "pruebas" account no longer works.
string? user = Environment.GetEnvironmentVariable("FACTURAMA_USER");
string? password = Environment.GetEnvironmentVariable("FACTURAMA_PASSWORD");
if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
{
    Console.Error.WriteLine("Set FACTURAMA_USER and FACTURAMA_PASSWORD (your Facturama Sandbox account).");

    return 1;
}

using var client = new FacturamaClient(user, password);

try
{
    JsonNode? result = await example.Run(client, args[1..]);
    Console.WriteLine(result?.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }) ?? "(empty response)");

    return 0;
}
catch (RequestException e)
{
    // The API message; validation details (ModelState) are chained as a ModelException.
    Console.Error.WriteLine($"Facturama rejected the request ({(int?)e.StatusCode}): {e.Message}");
    if (e.InnerException is ModelException model)
    {
        Console.Error.WriteLine($"  {model.Message}");
    }

    return 2;
}
catch (ResponseException e)
{
    Console.Error.WriteLine($"Unexpected response: {e.Message}");

    return 3;
}

static string Arg(string[] args, int index, string? fallback = null)
    => index < args.Length ? args[index] : fallback ?? throw new ArgumentException($"Missing argument #{index + 1}; run without arguments to see the usage.");
