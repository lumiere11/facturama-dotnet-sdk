using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Facturama.Exceptions;
using Facturama.Tests.Support;

namespace Facturama.Tests;

/// <summary>Offline tests: no network access required.</summary>
public sealed class FacturamaClientTests
{
    [Fact]
    public void UserAgentMatchesVersion()
    {
        Assert.Equal("Facturama-DotNet-SDK-v" + FacturamaClient.Version, FacturamaClient.UserAgent);
        Assert.Matches(@"^\d+\.\d+\.\d+", FacturamaClient.Version);
        Assert.DoesNotContain('+', FacturamaClient.Version);
    }

    [Fact]
    public async Task GetDecodesJson()
    {
        using FacturamaClient client = CreateClient(StubHandler.Responding(HttpStatusCode.OK, """{"Name":"x"}"""));

        var result = await client.GetAsync("catalogs");

        Assert.Equal("x", (string?)result?["Name"]);
    }

    [Fact]
    public async Task EmptyBodyReturnsNull()
    {
        using FacturamaClient client = CreateClient(StubHandler.Responding(HttpStatusCode.OK, string.Empty));

        Assert.Null(await client.GetAsync("catalogs"));
    }

    [Fact]
    public async Task ErrorMessageIsExposed()
    {
        using FacturamaClient client = CreateClient(StubHandler.Responding(HttpStatusCode.BadRequest, """{"Message":"boom"}"""));

        var exception = await Assert.ThrowsAsync<RequestException>(() => client.PostAsync("cfdi", new { }));

        Assert.Equal("boom", exception.Message);
        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public async Task ModelStateMessagesAreChained()
    {
        const string Body = """{"Message":"invalid","ModelState":{"a":["bad A"],"b":["bad B"]}}""";
        using FacturamaClient client = CreateClient(StubHandler.Responding(HttpStatusCode.BadRequest, Body));

        var exception = await Assert.ThrowsAsync<RequestException>(() => client.PostAsync("cfdi", new { }));

        Assert.Equal("invalid", exception.Message);
        var model = Assert.IsType<ModelException>(exception.InnerException);
        Assert.Equal("bad A; bad B.", model.Message);
    }

    [Fact]
    public async Task EmptyModelStateStillChainsAModelException()
    {
        using FacturamaClient client = CreateClient(StubHandler.Responding(HttpStatusCode.BadRequest, """{"Message":"invalid","ModelState":{}}"""));

        var exception = await Assert.ThrowsAsync<RequestException>(() => client.PostAsync("cfdi", new { }));

        Assert.Equal(string.Empty, Assert.IsType<ModelException>(exception.InnerException).Message);
    }

    [Fact]
    public async Task NonJsonErrorBodyIsUsedAsMessage()
    {
        using FacturamaClient client = CreateClient(StubHandler.Responding(HttpStatusCode.InternalServerError, "Internal failure"));

        var exception = await Assert.ThrowsAsync<RequestException>(() => client.GetAsync("anything"));

        Assert.Equal("Internal failure", exception.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
    }

    [Fact]
    public async Task EmptyErrorBodyFallsBackToTheStatus()
    {
        using FacturamaClient client = CreateClient(StubHandler.Responding(HttpStatusCode.Unauthorized, string.Empty));

        var exception = await Assert.ThrowsAsync<RequestException>(() => client.GetAsync("anything"));

        Assert.Contains("401", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvalidJsonOnSuccessThrows()
    {
        using FacturamaClient client = CreateClient(StubHandler.Responding(HttpStatusCode.OK, "{not json"));

        var exception = await Assert.ThrowsAsync<ResponseException>(() => client.GetAsync("anything"));

        Assert.Contains("{not json", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryExceptionDerivesFromFacturamaException()
    {
        Assert.IsAssignableFrom<FacturamaException>(new RequestException("x"));
        Assert.IsAssignableFrom<FacturamaException>(new ResponseException("x"));
        Assert.IsAssignableFrom<FacturamaException>(new ModelException("x"));
    }

    [Fact]
    public async Task NetworkFailuresBecomeRequestExceptions()
    {
        var handler = new StubHandler().Enqueue(_ => throw new HttpRequestException("connection refused"));
        using FacturamaClient client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<RequestException>(() => client.GetAsync("anything"));

        Assert.Equal("connection refused", exception.Message);
        Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    [Fact]
    public async Task TimeoutsBecomeRequestExceptions()
    {
        var handler = new StubHandler().Enqueue(_ => throw new TaskCanceledException("timeout"));
        using FacturamaClient client = CreateClient(handler);

        await Assert.ThrowsAsync<RequestException>(() => client.GetAsync("anything"));
    }

    [Fact]
    public async Task CancellationIsNotWrapped()
    {
        using FacturamaClient client = CreateClient(new StubHandler());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetAsync("anything", cancellationToken: cts.Token));
    }

    [Fact]
    public async Task RequestUsesBaseUriWithoutDoubleSlash()
    {
        var handler = new StubHandler();
        using FacturamaClient client = CreateClient(handler);

        await client.GetAsync("catalogs/CfdiTypes", new Dictionary<string, object?> { ["keyword"] = "x" });
        await client.GetAsync("/Cfdi");

        Assert.Equal("https://apisandbox.facturama.mx/catalogs/CfdiTypes?keyword=x", handler.Requests[0].Uri.ToString());
        Assert.Equal("https://apisandbox.facturama.mx/Cfdi", handler.Requests[1].Uri.ToString());
    }

    [Fact]
    public async Task QueryValuesAreEncodedAndNullsSkipped()
    {
        var handler = new StubHandler();
        using FacturamaClient client = CreateClient(handler);

        await client.GetAsync("Clients", new Dictionary<string, object?>
        {
            ["search"] = "a b&c",
            ["start"] = 10,
            ["flag"] = true,
            ["price"] = 1.5m,
            ["skipped"] = null,
        });

        Assert.Equal("?search=a%20b%26c&start=10&flag=true&price=1.5", handler.Last.Uri.Query);
    }

    [Fact]
    public async Task SetApiUrlChangesBaseUri()
    {
        var handler = new StubHandler();
        using FacturamaClient client = CreateClient(handler);

        Assert.Same(client, client.SetApiUrl("https://api.facturama.mx/"));
        await client.DeleteAsync("cfdi/1");

        Assert.Equal(HttpMethod.Delete, handler.Last.Method);
        Assert.Equal("https://api.facturama.mx/cfdi/1", handler.Last.Uri.ToString());
    }

    [Fact]
    public async Task PostSendsJsonBody()
    {
        var handler = new StubHandler();
        using FacturamaClient client = CreateClient(handler);

        await client.PostAsync("cfdi", new { A = 1 });

        Assert.Equal("""{"A":1}""", handler.Last.Body);
        Assert.StartsWith("application/json", handler.Last.ContentType, StringComparison.Ordinal);
    }

    [Fact]
    public async Task JsonKeepsAccentsReadable()
    {
        var handler = new StubHandler();
        using FacturamaClient client = CreateClient(handler);

        await client.PostAsync("cfdi", new Dictionary<string, object?> { ["Name"] = "ORGANICOS ÑAVEZ OSORIO", ["Date"] = null });

        Assert.Equal("""{"Name":"ORGANICOS ÑAVEZ OSORIO","Date":null}""", handler.Last.Body);
    }

    [Fact]
    public async Task PostWithoutBodySendsAnEmptyArray()
    {
        var handler = new StubHandler();
        using FacturamaClient client = CreateClient(handler);

        await client.PostAsync("Cfdi");

        Assert.Equal("[]", handler.Last.Body);
    }

    [Fact]
    public async Task PutWithoutBodySendsNothing()
    {
        var handler = new StubHandler();
        using FacturamaClient client = CreateClient(handler);

        await client.PutAsync("Client/1");

        Assert.Null(handler.Last.Body);
    }

    [Fact]
    public async Task BasicAuthAndUserAgentAreSentPerRequest()
    {
        var handler = new StubHandler();
        using var http = handler.CreateHttpClient();
        using var client = new FacturamaClient(http, "user", "pass");

        await client.GetAsync("catalogs");

        Assert.Equal("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("user:pass")), handler.Last.Authorization);
        Assert.Equal(FacturamaClient.UserAgent, handler.Last.UserAgent);
        // The injected HttpClient must not be mutated.
        Assert.Null(http.DefaultRequestHeaders.Authorization);
        Assert.Empty(http.DefaultRequestHeaders.UserAgent);
    }

    [Fact]
    public async Task NoCredentialsMeansNoAuthorizationHeader()
    {
        var handler = new StubHandler();
        using FacturamaClient client = CreateClient(handler);

        await client.GetAsync("catalogs");

        Assert.Null(handler.Last.Authorization);
    }

    [Fact]
    public void DefaultsToSandboxAndHonoursOptions()
    {
        using var sandbox = new FacturamaClient();
        Assert.False(sandbox.IsProduction);
        Assert.Equal(FacturamaClient.SandboxUrl, sandbox.ApiUrl);

        using var production = new FacturamaClient(new FacturamaOptions { Environment = FacturamaEnvironment.Production });
        Assert.True(production.IsProduction);

        using var custom = new FacturamaClient(new FacturamaOptions { ApiUrl = "https://example.test/" });
        Assert.Equal("https://example.test", custom.ApiUrl);
        Assert.False(custom.IsProduction);
    }

    [Fact]
    public void IsProductionFollowsSetApiUrl()
    {
        using var client = new FacturamaClient();

        Assert.True(client.SetApiUrl(FacturamaClient.ProductionUrl + "/").IsProduction);
    }

    [Fact]
    public async Task InjectedHttpClientIsNotDisposed()
    {
        var handler = new StubHandler();
        using var http = handler.CreateHttpClient();
        var client = new FacturamaClient(http);
        client.Dispose();

        // Still usable after the SDK client is disposed.
        using var other = new FacturamaClient(http);
        await other.GetAsync("catalogs");

        Assert.Single(handler.Requests);
    }

    [Fact]
    public void VersionMatchesDirectoryBuildProps()
    {
        string? directory = AppContext.BaseDirectory;
        while (directory is not null && !File.Exists(Path.Combine(directory, "Directory.Build.props")))
        {
            directory = Path.GetDirectoryName(directory);
        }

        Assert.NotNull(directory);
        string props = File.ReadAllText(Path.Combine(directory, "Directory.Build.props"));
        Match match = Regex.Match(props, "<Version>([^<]+)</Version>");

        Assert.True(match.Success);
        Assert.Equal(match.Groups[1].Value, FacturamaClient.Version);
    }

    private static FacturamaClient CreateClient(StubHandler handler)
        => new(handler.CreateHttpClient());
}
