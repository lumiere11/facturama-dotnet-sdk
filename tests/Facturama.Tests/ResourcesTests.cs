using System.Net;
using System.Text;
using System.Web;
using Facturama.Exceptions;
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
    public async Task CfdiDownloadBytesDecodesTheContent()
    {
        StubHandler handler = new StubHandler().Enqueue(HttpStatusCode.OK, """{"ContentEncoding":"base64","ContentType":"xml","Content":"PGNmZGkvPg=="}""");
        using FacturamaClient client = new(handler.CreateHttpClient(), "u", "p");

        byte[] content = await client.Cfdis.DownloadBytesAsync("abc", FileTypes.Xml);

        Assert.Equal("<cfdi/>", Encoding.UTF8.GetString(content));
        Assert.Equal("/cfdi/xml/issued/abc", handler.Last.Uri.AbsolutePath);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("""{"Id":"x"}""")]
    [InlineData("""{"Content":null}""")]
    [InlineData("""{"Content":5}""")]
    [InlineData("""{"Content":"not base64!"}""")]
    public async Task CfdiDownloadBytesRejectsAResponseWithoutContent(string body)
    {
        StubHandler handler = new StubHandler().Enqueue(HttpStatusCode.OK, body);
        using FacturamaClient client = new(handler.CreateHttpClient(), "u", "p");

        await Assert.ThrowsAsync<ResponseException>(() => client.Cfdis.DownloadBytesAsync("abc"));
    }

    [Fact]
    public async Task CfdiDownloadToFileWritesTheDecodedContent()
    {
        StubHandler handler = new StubHandler().Enqueue(HttpStatusCode.OK, """{"Content":"PGNmZGkvPg=="}""");
        using FacturamaClient client = new(handler.CreateHttpClient(), "u", "p");
        string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            await client.Cfdis.DownloadToFileAsync("abc", path, FileTypes.Xml, ReceiptTypes.Received);

            Assert.Equal("<cfdi/>", await File.ReadAllTextAsync(path));
            Assert.Equal("/cfdi/xml/received/abc", handler.Last.Uri.AbsolutePath);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task MultiissuerCfdiCreateFindAndList()
    {
        using FacturamaClient client = CreateClient();

        await client.Multiissuer.Cfdis.CreateAsync(new { CfdiType = "I" });
        Assert.Equal(HttpMethod.Post, _handler.Last.Method);
        Assert.Equal("/api-lite/3/cfdis", _handler.Last.Uri.AbsolutePath);
        Assert.Equal("""{"CfdiType":"I"}""", _handler.Last.Body);

        await client.Multiissuer.Cfdis.FindAsync("a/b");
        Assert.Equal(HttpMethod.Get, _handler.Last.Method);
        Assert.Equal("/api-lite/cfdis/a%2Fb", _handler.Last.Uri.AbsolutePath);

        await client.Multiissuer.Cfdis.ListAsync();
        Assert.Equal("/api-lite/cfdis", _handler.Last.Uri.AbsolutePath);
        Assert.Equal(string.Empty, _handler.Last.Uri.Query);

        await client.Multiissuer.Cfdis.ListAsync(new Dictionary<string, object?> { ["keyword"] = "abc" });
        Assert.Equal("?keyword=abc", _handler.Last.Uri.Query);
    }

    [Fact]
    public async Task MultiissuerCfdiDownloadsUseTheIssuedLiteType()
    {
        StubHandler handler = StubHandler.Responding(HttpStatusCode.OK, "{}").Enqueue(HttpStatusCode.OK, """{"Content":"PGNmZGkvPg=="}""").Enqueue(HttpStatusCode.OK, """{"Content":"PGNmZGkvPg=="}""");
        using FacturamaClient client = new(handler.CreateHttpClient(), "u", "p");
        string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        await client.Multiissuer.Cfdis.DownloadAsync("abc");
        Assert.Equal("/cfdi/pdf/issuedLite/abc", handler.Last.Uri.AbsolutePath);

        byte[] content = await client.Multiissuer.Cfdis.DownloadBytesAsync("abc", FileTypes.Xml);
        Assert.Equal("<cfdi/>", Encoding.UTF8.GetString(content));
        Assert.Equal("/cfdi/xml/issuedLite/abc", handler.Last.Uri.AbsolutePath);

        try
        {
            await client.Multiissuer.Cfdis.DownloadToFileAsync("abc", path, FileTypes.Xml);

            Assert.Equal("<cfdi/>", await File.ReadAllTextAsync(path));
            Assert.Equal("/cfdi/xml/issuedLite/abc", handler.Last.Uri.AbsolutePath);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task MultiissuerCfdiSendByEmail()
    {
        using FacturamaClient client = CreateClient();

        await client.Multiissuer.Cfdis.SendByEmailAsync("abc", "a@b.mx");

        Assert.Equal(HttpMethod.Post, _handler.Last.Method);
        Assert.Equal("/Cfdi", _handler.Last.Uri.AbsolutePath);
        Assert.Equal("?cfdiType=issuedLite&cfdiId=abc&email=a%40b.mx", _handler.Last.Uri.Query);
    }

    [Fact]
    public async Task MultiissuerCfdiCancel()
    {
        using FacturamaClient client = CreateClient();

        await client.Multiissuer.Cfdis.CancelAsync("abc", "02");
        Assert.Equal(HttpMethod.Delete, _handler.Last.Method);
        Assert.Equal("/api-lite/cfdis/abc", _handler.Last.Uri.AbsolutePath);
        Assert.Equal("?type=issuedLite&motive=02", _handler.Last.Uri.Query);

        await client.Multiissuer.Cfdis.CancelAsync("abc", "01", "UUID-1");
        Assert.Equal("?type=issuedLite&motive=01&uuidReplacement=UUID-1", _handler.Last.Uri.Query);
    }

    [Fact]
    public async Task CsdsCrudRequests()
    {
        using FacturamaClient client = CreateClient();
        var payload = new { Rfc = "EKU9003173C9" };

        await client.Multiissuer.Csds.ListAsync();
        Assert.Equal(HttpMethod.Get, _handler.Last.Method);
        Assert.Equal("/api-lite/csds", _handler.Last.Uri.AbsolutePath);

        await client.Multiissuer.Csds.FindAsync("EKU9003173C9");
        Assert.Equal(HttpMethod.Get, _handler.Last.Method);
        Assert.Equal("/api-lite/csds/EKU9003173C9", _handler.Last.Uri.AbsolutePath);

        await client.Multiissuer.Csds.CreateAsync(payload);
        Assert.Equal(HttpMethod.Post, _handler.Last.Method);
        Assert.Equal("/api-lite/csds", _handler.Last.Uri.AbsolutePath);
        Assert.Equal("""{"Rfc":"EKU9003173C9"}""", _handler.Last.Body);

        await client.Multiissuer.Csds.UpdateAsync("EKU9003173C9", payload);
        Assert.Equal(HttpMethod.Put, _handler.Last.Method);
        Assert.Equal("/api-lite/csds/EKU9003173C9", _handler.Last.Uri.AbsolutePath);
        Assert.Equal("""{"Rfc":"EKU9003173C9"}""", _handler.Last.Body);

        await client.Multiissuer.Csds.DeleteAsync("EKU9003173C9");
        Assert.Equal(HttpMethod.Delete, _handler.Last.Method);
        Assert.Equal("/api-lite/csds/EKU9003173C9", _handler.Last.Uri.AbsolutePath);
    }

    [Fact]
    public async Task CsdsFromFilesEncodeTheCertificateAndKey()
    {
        using FacturamaClient client = CreateClient();
        string certificate = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        string privateKey = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        const string Expected = """{"Rfc":"EKU9003173C9","Certificate":"Y2Vy","PrivateKey":"a2V5","PrivateKeyPassword":"secret"}""";

        try
        {
            await File.WriteAllTextAsync(certificate, "cer");
            await File.WriteAllTextAsync(privateKey, "key");

            await client.Multiissuer.Csds.CreateFromFilesAsync("EKU9003173C9", certificate, privateKey, "secret");
            Assert.Equal(HttpMethod.Post, _handler.Last.Method);
            Assert.Equal("/api-lite/csds", _handler.Last.Uri.AbsolutePath);
            Assert.Equal(Expected, _handler.Last.Body);

            await client.Multiissuer.Csds.UpdateFromFilesAsync("EKU9003173C9", certificate, privateKey, "secret");
            Assert.Equal(HttpMethod.Put, _handler.Last.Method);
            Assert.Equal("/api-lite/csds/EKU9003173C9", _handler.Last.Uri.AbsolutePath);
            Assert.Equal(Expected, _handler.Last.Body);
        }
        finally
        {
            File.Delete(certificate);
            File.Delete(privateKey);
        }
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
