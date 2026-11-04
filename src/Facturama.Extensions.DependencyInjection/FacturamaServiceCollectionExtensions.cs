using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Facturama.Extensions.DependencyInjection;

/// <summary><see cref="IServiceCollection"/> extensions that register a shared <see cref="FacturamaClient"/>.</summary>
public static class FacturamaServiceCollectionExtensions
{
    /// <summary>The name of the <see cref="HttpClient"/> registered for the Facturama client.</summary>
    public const string HttpClientName = "Facturama";

    /// <summary>
    /// Registers a singleton <see cref="FacturamaClient"/> configured with <paramref name="configure"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Sets the credentials, environment and timeouts.</param>
    /// <returns>
    /// The <see cref="IHttpClientBuilder"/> of the underlying <see cref="HttpClient"/> (named <see cref="HttpClientName"/>),
    /// so that handlers (logging, resilience, ...) can be added to it.
    /// </returns>
    public static IHttpClientBuilder AddFacturama(this IServiceCollection services, Action<FacturamaOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<FacturamaOptions>().Configure(configure);

        return AddCore(services);
    }

    /// <summary>
    /// Registers a singleton <see cref="FacturamaClient"/> configured from a configuration section, e.g.
    /// <c>{ "Facturama": { "User": "...", "Password": "...", "Environment": "Sandbox" } }</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The section holding <see cref="FacturamaOptions"/> (typically <c>configuration.GetSection("Facturama")</c>).</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> of the underlying <see cref="HttpClient"/>.</returns>
    public static IHttpClientBuilder AddFacturama(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<FacturamaOptions>().Bind(configuration);

        return AddCore(services);
    }

    private static IHttpClientBuilder AddCore(IServiceCollection services)
    {
        IHttpClientBuilder builder = services
            .AddHttpClient(HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(sp => new SocketsHttpHandler
            {
                ConnectTimeout = sp.GetRequiredService<IOptions<FacturamaOptions>>().Value.ConnectTimeout,
                // The client is a singleton, so it must not pin a pooled connection (and its DNS answer) forever.
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            })
            .ConfigureHttpClient((sp, http) => http.Timeout = sp.GetRequiredService<IOptions<FacturamaOptions>>().Value.Timeout)
            // A singleton keeps its handler chain: connection rotation is handled by PooledConnectionLifetime above.
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan);

        services.TryAddSingleton(sp => new FacturamaClient(
            sp.GetRequiredService<IOptions<FacturamaOptions>>().Value,
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));

        return builder;
    }
}
