using System.Net;
using Facturama.Extensions.DependencyInjection;
using Facturama.Tests.Support;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Facturama.Tests;

/// <summary>Offline tests for <see cref="FacturamaServiceCollectionExtensions"/> (the .NET counterpart of the Laravel integration).</summary>
public sealed class DependencyInjectionTests
{
    [Fact]
    public void RegistersASharedSandboxClientByDefault()
    {
        var services = new ServiceCollection();
        services.AddFacturama(options =>
        {
            options.User = "u";
            options.Password = "p";
        });
        using ServiceProvider provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<FacturamaClient>();

        Assert.Same(client, provider.GetRequiredService<FacturamaClient>());
        Assert.False(client.IsProduction);
        Assert.Equal(FacturamaClient.SandboxUrl, client.ApiUrl);
    }

    [Fact]
    public async Task SendsTheConfiguredCredentials()
    {
        var handler = new StubHandler();
        var services = new ServiceCollection();
        services.AddFacturama(options =>
        {
            options.User = "u";
            options.Password = "p";
        }).ConfigurePrimaryHttpMessageHandler(() => handler);
        using ServiceProvider provider = services.BuildServiceProvider();

        await provider.GetRequiredService<FacturamaClient>().Clients.ListAsync();

        Assert.Equal("Basic dTpw", handler.Last.Authorization);
        Assert.Equal("/Clients", handler.Last.Uri.AbsolutePath);
    }

    [Fact]
    public void BindsFromConfiguration()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Facturama:User"] = "u",
                ["Facturama:Password"] = "p",
                ["Facturama:Environment"] = "production",
                ["Facturama:Timeout"] = "00:00:30",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddFacturama(configuration.GetSection("Facturama"));
        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.True(provider.GetRequiredService<FacturamaClient>().IsProduction);
    }

    [Fact]
    public void UnknownEnvironmentIsRejectedWhenBinding()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Facturama:Environment"] = "staging" })
            .Build();
        var services = new ServiceCollection();
        services.AddFacturama(configuration.GetSection("Facturama"));
        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.ThrowsAny<Exception>(() => provider.GetRequiredService<FacturamaClient>());
    }

    [Fact]
    public async Task HttpClientCanBeCustomisedThroughTheBuilder()
    {
        var handler = new StubHandler().Enqueue(HttpStatusCode.OK, "[]");
        var services = new ServiceCollection();
        services.AddFacturama(options => options.ApiUrl = "https://example.test")
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        using ServiceProvider provider = services.BuildServiceProvider();

        await provider.GetRequiredService<FacturamaClient>().GetAsync("ping");

        Assert.Equal("https://example.test/ping", handler.Last.Uri.ToString());
    }
}
