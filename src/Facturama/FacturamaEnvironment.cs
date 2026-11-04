namespace Facturama;

/// <summary>The Facturama environment a client talks to.</summary>
public enum FacturamaEnvironment
{
    /// <summary>Test environment (<c>https://apisandbox.facturama.mx</c>). Stamps test CFDIs only. This is the default.</summary>
    Sandbox = 0,

    /// <summary>Production environment (<c>https://api.facturama.mx</c>). Stamps real CFDIs.</summary>
    Production = 1,
}
