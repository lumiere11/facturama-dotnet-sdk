using System.Text.Json.Nodes;

namespace Facturama.Examples.Web;

/// <summary>Product CRUD: create, list, find, update and delete (everything runs against the Sandbox).</summary>
internal static class ProductExamples
{
    public static async Task<JsonNode?> RunAsync(FacturamaClient client)
    {
        JsonNode? created = await client.Products.CreateAsync(Payload(id: null, price: 6500));
        string id = created!["Id"]!.GetValue<string>();
        Console.WriteLine($"Created product {id}");

        try
        {
            JsonNode? list = await client.Products.ListAsync(search: "Sitio Web");
            Console.WriteLine($"Search found {list?.AsArray().Count} product(s)");

            JsonNode? found = await client.Products.FindAsync(id);
            Console.WriteLine($"Found {found!["Name"]}");

            await client.Products.UpdateAsync(id, Payload(id, price: 7000));
            Console.WriteLine("Updated");

            return await client.Products.FindAsync(id);
        }
        finally
        {
            await client.Products.DeleteAsync(id);
            Console.WriteLine("Deleted");
        }
    }

    private static object Payload(string? id, decimal price) => new
    {
        Id = id,
        Unit = "Servicio",
        UnitCode = "E48",
        Name = "Sitio Web CMS",
        IdentificationNumber = "WEB003",
        Description = "Desarrollo e implementación de sitio web empleando un CMS",
        Price = price,
        CodeProdServ = "43232408",
        CuentaPredial = "123",
        Taxes = new[]
        {
            new { Name = "IVA", Rate = 0.16m, IsRetention = false, IsFederalTax = true },
        },
    };
}
