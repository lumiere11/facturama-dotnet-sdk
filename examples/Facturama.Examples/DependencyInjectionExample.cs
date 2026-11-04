using System.Text.Json.Nodes;
using Facturama.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Facturama.Examples;

/// <summary>
/// Dependency injection with <c>AddFacturama</c>. In an ASP.NET Core app this is
/// <c>builder.Services.AddFacturama(builder.Configuration.GetSection("Facturama"))</c> with, in appsettings.json or
/// user secrets: <c>{ "Facturama": { "User": "...", "Password": "...", "Environment": "Sandbox" } }</c>.
/// A controller (or minimal API handler) then just takes a <see cref="FacturamaClient"/> parameter.
/// </summary>
internal static class DependencyInjectionExample
{
    public static async Task<JsonNode?> RunAsync()
    {
        // In a real app this is the application's IConfiguration (appsettings.json, user secrets, environment variables).
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Facturama:User"] = Environment.GetEnvironmentVariable("FACTURAMA_USER"),
                ["Facturama:Password"] = Environment.GetEnvironmentVariable("FACTURAMA_PASSWORD"),
                ["Facturama:Environment"] = "Sandbox",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddFacturama(configuration.GetSection("Facturama"));
        services.AddTransient<InvoiceService>();

        await using ServiceProvider provider = services.BuildServiceProvider();

        // The container always returns the same configured client.
        return await provider.GetRequiredService<InvoiceService>().ListClientsAsync();
    }

    /// <summary>A consumer that receives the shared client by constructor injection.</summary>
    private sealed class InvoiceService(FacturamaClient facturama)
    {
        public Task<JsonNode?> ListClientsAsync() => facturama.Clients.ListAsync();
    }
}
