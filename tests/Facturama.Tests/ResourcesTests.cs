using System.Net;
using System.Web;
using Facturama.Tests.Support;

namespace Facturama.Tests;

/// <summary>Offline tests: the resources only build the right HTTP requests.</summary>
public sealed class ResourcesTests
{
    private readonly StubHandler _handler = StubHandler.Responding(HttpStatusCode.OK, """{"Id":"x"}""");

    [Fact]
    public async Task CfdiCreatePostsTo3Cfdis()
    {
        using FacturamaClient client = CreateClient();

        var result = await client.Cfdis.CreateAsync(new { CfdiType = "I" });

        Assert.Equal("x", (string?)result?["Id"]);
        Assert.Equal(HttpMethod.Post, _handler.Last.Method);
        Assert.Equal("/3/cfdis", _handler.Last.Uri.AbsolutePath);
        Assert.Equal("""{"CfdiType":"I"}""", _handler.Last.Body);
    }

    [Fact]
    public async Task CfdiListDefaultsToIssued()
    {
        using FacturamaClient client = CreateClient();

        await client.Cfdis.ListAsync(new Dictionary<string, object?> { ["page"] = 2 });

        Assert.Equal("/Cfdi", _handler.Last.Uri.AbsolutePath);
        Assert.Equal("?page=2&type=issued", _handler.Last.Uri.Query);
    }

    [Fact]
    public async Task CfdiListKeepsAnExplicitType()
    {
        using FacturamaClient client = CreateClient();

        await client.Cfdis.ListAsync(new Dictionary<string, object?> { ["type"] = ReceiptTypes.Received });

        Assert.Equal("?type=received", _handler.Last.Uri.Query);
    }

    [Fact]
    public async Task CfdiListWithoutFilters()
    {
        using FacturamaClient client = CreateClient();

        await client.Cfdis.ListAsync();

        Assert.Equal("?type=issued", _handler.Last.Uri.Query);
    }

    [Fact]
    public async Task CfdiDownload()
    {
        using FacturamaClient client = CreateClient();

        await client.Cfdis.DownloadAsync("abc", FileTypes.Xml, ReceiptTypes.Received);

        Assert.Equal("/cfdi/xml/received/abc", _handler.Last.Uri.AbsolutePath);
    }

    [Fact]
    public async Task CfdiDownloadDefaultsToIssuedPdf()
    {
        using FacturamaClient client = CreateClient();

        await client.Cfdis.DownloadAsync("abc");

        Assert.Equal("/cfdi/pdf/issued/abc", _handler.Last.Uri.AbsolutePath);
    }

    [Fact]
    public async Task CfdiSendByEmail()
    {
        using FacturamaClient client = CreateClient();

        await client.Cfdis.SendByEmailAsync("abc", "a@b.mx");

        Assert.Equal(HttpMethod.Post, _handler.Last.Method);
        Assert.Equal("/Cfdi", _handler.Last.Uri.AbsolutePath);
        Assert.Equal("[]", _handler.Last.Body);
        var query = HttpUtility.ParseQueryString(_handler.Last.Uri.Query);
        Assert.Equal("issued", query["cfdiType"]);
        Assert.Equal("abc", query["cfdiId"]);
        Assert.Equal("a@b.mx", query["email"]);
    }

    [Fact]
    public async Task CfdiCancelWithSubstitution()
    {
        using FacturamaClient client = CreateClient();

        await client.Cfdis.CancelAsync("abc", "01", ReceiptTypes.Issued, "UUID-1");

        Assert.Equal(HttpMethod.Delete, _handler.Last.Method);
        Assert.Equal("/Cfdi/abc", _handler.Last.Uri.AbsolutePath);
        Assert.Equal("?type=issued&motive=01&uuidReplacement=UUID-1", _handler.Last.Uri.Query);
    }

    [Fact]
    public async Task CfdiCancelWithoutSubstitution()
    {
        using FacturamaClient client = CreateClient();

        await client.Cfdis.CancelAsync("abc", "02");

        Assert.Equal("?type=issued&motive=02", _handler.Last.Uri.Query);
    }

    [Fact]
    public async Task IdsAreEscaped()
    {
        using FacturamaClient client = CreateClient();

        await client.Clients.FindAsync("a/b c");

        Assert.Equal("/Client/a%2Fb%20c", _handler.Last.Uri.AbsolutePath);
    }

    [Fact]
    public Task ClientsCrudRequests()
    {
        FacturamaClient client = CreateClient();

        return AssertCrudAsync(
            "Clients",
            "Client",
            new Crud(
                () => client.Clients.ListAsync(),
                id => client.Clients.FindAsync(id),
                data => client.Clients.CreateAsync(data),
                (id, data) => client.Clients.UpdateAsync(id, data),
                id => client.Clients.DeleteAsync(id)));
    }

    [Fact]
    public Task ProductsCrudRequests()
    {
        FacturamaClient client = CreateClient();

        return AssertCrudAsync(
            "Products",
            "Product",
            new Crud(
                () => client.Products.ListAsync(),
                id => client.Products.FindAsync(id),
                data => client.Products.CreateAsync(data),
                (id, data) => client.Products.UpdateAsync(id, data),
                id => client.Products.DeleteAsync(id)));
    }

    private async Task AssertCrudAsync(string collection, string item, Crud crud)
    {
        var payload = new { Name = "n" };

        await crud.List();
        Assert.Equal(HttpMethod.Get, _handler.Last.Method);
        Assert.Equal("/" + collection, _handler.Last.Uri.AbsolutePath);
        Assert.Equal("?search=&start=0&length=100", _handler.Last.Uri.Query);

        await crud.Find("1");
        Assert.Equal(HttpMethod.Get, _handler.Last.Method);
        Assert.Equal($"/{item}/1", _handler.Last.Uri.AbsolutePath);

        await crud.Create(payload);
        Assert.Equal(HttpMethod.Post, _handler.Last.Method);
        Assert.Equal("/" + item, _handler.Last.Uri.AbsolutePath);
        Assert.Equal("""{"Name":"n"}""", _handler.Last.Body);

        await crud.Update("1", payload);
        Assert.Equal(HttpMethod.Put, _handler.Last.Method);
        Assert.Equal($"/{item}/1", _handler.Last.Uri.AbsolutePath);
        Assert.Equal("""{"Name":"n"}""", _handler.Last.Body);

        await crud.Delete("1");
        Assert.Equal(HttpMethod.Delete, _handler.Last.Method);
        Assert.Equal($"/{item}/1", _handler.Last.Uri.AbsolutePath);

        Assert.Equal(5, _handler.Requests.Count);
    }

    private FacturamaClient CreateClient() => new(_handler.CreateHttpClient(), "u", "p");

    /// <summary><c>Clients</c> and <c>Products</c> expose the same CRUD surface; this adapts both to one shape.</summary>
    private sealed record Crud(
        Func<Task> List,
        Func<string, Task> Find,
        Func<object, Task> Create,
        Func<string, object, Task> Update,
        Func<string, Task> Delete);
}
