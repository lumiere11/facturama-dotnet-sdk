namespace Facturama.Resources;

/// <summary>
/// The Multi-issuer API (<c>api-lite</c>): CFDIs stamped on behalf of several issuers, each with its own CSD.
/// These operations are not reflected in Facturama's web app.
/// </summary>
public sealed class Multiissuer
{
    private readonly FacturamaClient _client;

    internal Multiissuer(FacturamaClient client) => _client = client;

    /// <summary>Gets the CFDI 4.0 operations of the Multi-issuer API.</summary>
    public MultiissuerCfdis Cfdis => field ??= new MultiissuerCfdis(_client);

    /// <summary>Gets the operations for the issuers' CSDs (digital seal certificates).</summary>
    public Csds Csds => field ??= new Csds(_client);
}
