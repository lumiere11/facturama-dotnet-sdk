using System.Text.Json.Nodes;
using Facturama.Model.Catalog;
using Facturama.Tests.Support;

namespace Facturama.Tests.Model.Catalog;

/// <summary>
/// Compares the local catalogs against the live Sandbox API. They need network access and valid credentials
/// (see <see cref="LiveFactAttribute"/>) and can fail when the remote catalog changes.
/// </summary>
[Trait("Category", "Live")]
public sealed class CatalogsLiveTests
{
    [LiveFact]
    public async Task CfdiUsesMatchTheApi()
    {
        using FacturamaClient client = LiveApi.CreateClient();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        // RFC with 12 chars (moral) and with 13 chars (natural) return different subsets.
        foreach (string keyword in new[] { "AAAA12345678", "AAAA123456789" })
        {
            foreach (JsonNode? item in await LiveApi.GetCatalogAsync(client, "catalogs/CfdiUses", keyword))
            {
                string name = item!["Name"]!.GetValue<string>();
                CfdiUseEntry local = CfdiUse.FindByName(name);
                Assert.Equal(item["Value"]!.GetValue<string>(), local.Value);
                Assert.Equal(item["Moral"]!.GetValue<bool>(), local.Moral);
                Assert.Equal(item["Natural"]!.GetValue<bool>(), local.Natural);
                seen.Add(name);
            }
        }

        Assert.Empty(CfdiUse.Entries.Select(entry => entry.Name).Except(seen));
    }

    [LiveFact]
    public async Task CfdiTypesMatchTheApi()
    {
        using FacturamaClient client = LiveApi.CreateClient();
        JsonArray items = await LiveApi.GetCatalogAsync(client, "catalogs/CfdiTypes");

        Assert.NotEmpty(items);
        foreach (JsonNode? item in items)
        {
            string name = item!["Name"]!.GetValue<string>();
            string value = item["Value"]!.GetValue<string>();
            int nameId = item["NameId"]!.GetValue<int>();

            Assert.Equal(name, CfdiType.FindByName(name).Name);
            Assert.Contains(new CfdiTypeEntry(name, value, nameId), CfdiType.FindAllByValue(value));
        }

        // CFDI types are tied to the configured account, so an "unused entries" check would be unreliable.
    }

    [LiveFact]
    public Task FederalTaxesMatchTheApi() => AssertPlainCatalogAsync<FederalTax>("catalogs/FederalTaxes");

    [LiveFact]
    public Task PaymentFormsMatchTheApi() => AssertPlainCatalogAsync<PaymentForm>("catalogs/PaymentForms");

    [LiveFact]
    public Task PaymentMethodsMatchTheApi() => AssertPlainCatalogAsync<PaymentMethod>("catalogs/PaymentMethods");

    private static async Task AssertPlainCatalogAsync<TCatalog>(string path)
        where TCatalog : ICatalog<CatalogEntry>
    {
        using FacturamaClient client = LiveApi.CreateClient();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        JsonArray items = await LiveApi.GetCatalogAsync(client, path);
        Assert.NotEmpty(items);
        foreach (JsonNode? item in items)
        {
            string name = item!["Name"]!.GetValue<string>();
            string value = item["Value"]!.GetValue<string>();

            Assert.Equal(value, TCatalog.FindByName(name).Value);
            Assert.Equal(value, TCatalog.FindByValue(value).Value);
            seen.Add(name);
        }

        Assert.Empty(TCatalog.Entries.Select(entry => entry.Name).Except(seen));
    }
}
