namespace Facturama;

/// <summary>Settings for a <see cref="FacturamaClient"/>.</summary>
public sealed class FacturamaOptions
{
    /// <summary>Gets or sets the Facturama account user (HTTP basic auth).</summary>
    public string? User { get; set; }

    /// <summary>Gets or sets the Facturama account password (HTTP basic auth).</summary>
    public string? Password { get; set; }

    /// <summary>Gets or sets the environment to use. Defaults to <see cref="FacturamaEnvironment.Sandbox"/>.</summary>
    public FacturamaEnvironment Environment { get; set; } = FacturamaEnvironment.Sandbox;

    /// <summary>Gets or sets an explicit base URL that takes precedence over <see cref="Environment"/>.</summary>
    public string? ApiUrl { get; set; }

    /// <summary>
    /// Gets or sets the time to wait while establishing a connection (default 10 seconds).
    /// Only applies when the client creates its own <see cref="HttpClient"/>.
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets the overall request timeout (default 60 seconds).
    /// Only applies when the client creates its own <see cref="HttpClient"/>.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);

    internal string ResolveApiUrl() => !string.IsNullOrWhiteSpace(ApiUrl)
        ? ApiUrl
        : Environment switch
        {
            FacturamaEnvironment.Sandbox => FacturamaClient.SandboxUrl,
            FacturamaEnvironment.Production => FacturamaClient.ProductionUrl,
            _ => throw new ArgumentOutOfRangeException(nameof(Environment), Environment, "Unknown Facturama environment; use Sandbox or Production."),
        };
}
