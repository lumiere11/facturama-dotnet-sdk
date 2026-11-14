using System.Text.Json.Nodes;

namespace Facturama.Resources;

/// <summary>The account's fiscal profile: tax data, CSD, FIEL and logo (<c>TaxEntity</c> endpoints).</summary>
public sealed class TaxEntity
{
    private readonly FacturamaClient _client;

    internal TaxEntity(FacturamaClient client) => _client = client;

    /// <summary>Gets the fiscal profile (<c>GET TaxEntity</c>).</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The fiscal profile.</returns>
    public Task<JsonNode?> FindAsync(CancellationToken cancellationToken = default)
        => _client.GetAsync("TaxEntity", cancellationToken: cancellationToken);

    /// <summary>Updates the fiscal profile (<c>PUT TaxEntity</c>).</summary>
    /// <param name="data">The profile (<c>Rfc</c>, <c>TaxName</c>, <c>FiscalRegime</c>, <c>TaxAddress</c>, ...).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> UpdateAsync(object data, CancellationToken cancellationToken = default)
        => _client.PutAsync("TaxEntity", data, cancellationToken: cancellationToken);

    /// <summary>Uploads the account's CSD (<c>PUT TaxEntity/UploadCsd</c>).</summary>
    /// <param name="data">The payload: <c>Certificate</c> and <c>PrivateKey</c> (base64) and <c>PrivateKeyPassword</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> UploadCsdAsync(object data, CancellationToken cancellationToken = default)
        => _client.PutAsync("TaxEntity/UploadCsd", data, cancellationToken: cancellationToken);

    /// <summary>Uploads the account's CSD from the <c>.cer</c> and <c>.key</c> files the SAT delivers (<c>PUT TaxEntity/UploadCsd</c>).</summary>
    /// <param name="certificatePath">The path of the <c>.cer</c> file.</param>
    /// <param name="privateKeyPath">The path of the <c>.key</c> file.</param>
    /// <param name="privateKeyPassword">The password of the private key.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public async Task<JsonNode?> UploadCsdFromFilesAsync(string certificatePath, string privateKeyPath, string privateKeyPassword, CancellationToken cancellationToken = default)
        => await UploadCsdAsync(await CsdFiles.ReadAsync(null, certificatePath, privateKeyPath, privateKeyPassword, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);

    /// <summary>Uploads the account's FIEL (<c>PUT TaxEntity/UploadFiel</c>).</summary>
    /// <param name="data">The payload: <c>Rfc</c>, <c>Certificate</c> and <c>PrivateKey</c> (base64) and <c>PrivateKeyPassword</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> UploadFielAsync(object data, CancellationToken cancellationToken = default)
        => _client.PutAsync("TaxEntity/UploadFiel", data, cancellationToken: cancellationToken);

    /// <summary>Uploads the logo printed on the CFDIs (<c>PUT TaxEntity/UploadLogo</c>).</summary>
    /// <param name="data">The payload: <c>Image</c> (base64 or a URL) and <c>Type</c> (e.g. <c>png</c>).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> UploadLogoAsync(object data, CancellationToken cancellationToken = default)
        => _client.PutAsync("TaxEntity/UploadLogo", data, cancellationToken: cancellationToken);
}
