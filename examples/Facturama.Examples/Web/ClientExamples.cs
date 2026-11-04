using System.Text.Json.Nodes;

namespace Facturama.Examples.Web;

/// <summary>Client CRUD: create, list, find, update and delete (everything runs against the Sandbox).</summary>
internal static class ClientExamples
{
    public static async Task<JsonNode?> RunAsync(FacturamaClient client)
    {
        JsonNode? created = await client.Clients.CreateAsync(Payload(id: string.Empty, name: "XOCHILT CASAS CHAVEZ"));
        string id = created!["Id"]!.GetValue<string>();
        Console.WriteLine($"Created client {id}");

        try
        {
            JsonNode? list = await client.Clients.ListAsync(search: "XOCHILT");
            Console.WriteLine($"Search found {list?.AsArray().Count} client(s)");

            JsonNode? found = await client.Clients.FindAsync(id);
            Console.WriteLine($"Found {found!["Name"]}");

            await client.Clients.UpdateAsync(id, Payload(id, "XOCHILT CASAS CHAVEZ (ACTUALIZADO)"));
            Console.WriteLine("Updated");

            return await client.Clients.FindAsync(id);
        }
        finally
        {
            await client.Clients.DeleteAsync(id);
            Console.WriteLine("Deleted");
        }
    }

    private static object Payload(string id, string name) => new
    {
        Id = id,
        Rfc = "CACX7605101P8",
        Name = name,
        Email = "test@facturma.com",
        FiscalRegime = "601",
        CfdiUse = "G03",
        TaxZipCode = "10740",
        TaxResidence = "10740",
        NumRegIdTrib = "131494-1055",
        Address = new
        {
            Street = "Calle de Pruebas",
            ExteriorNumber = "123",
            InteriorNumber = "456",
            Neighborhood = "Colombia",
            ZipCode = "10740",
            Locality = "Mexico",
            Municipality = "Mexico Mun",
            State = States.CiudadDeMexico,
            Country = "MX",
        },
    };
}
