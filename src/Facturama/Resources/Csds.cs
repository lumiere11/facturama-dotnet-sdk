using System.Text.Json.Nodes;

namespace Facturama.Resources;

/// <summary>The issuers' CSDs (digital seal certificates) of the Multi-issuer API (<c>api-lite/csds</c>).</summary>
public sealed class Csds
{
    private readonly FacturamaClient _client;

    internal Csds(FacturamaClient client) => _client = client;

    /// <summary>Lists the uploaded CSDs (<c>GET api-lite/csds</c>).</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The CSDs.</returns>
    public Task<JsonNode?> ListAsync(CancellationToken cancellationToken = default)
        => _client.GetAsync("api-lite/csds", cancellationToken: cancellationToken);

    /// <summary>Gets the CSD of an issuer (<c>GET api-lite/csds/{rfc}</c>).</summary>
    /// <param name="rfc">The issuer's RFC.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The CSD.</returns>
    public Task<JsonNode?> FindAsync(string rfc, CancellationToken cancellationToken = default)
        => _client.GetAsync(Path(rfc), cancellationToken: cancellationToken);

    /// <summary>Uploads the CSD of an issuer (<c>POST api-lite/csds</c>).</summary>
    /// <param name="data">The payload: <c>Rfc</c>, <c>Certificate</c> and <c>PrivateKey</c> (base64) and <c>PrivateKeyPassword</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> CreateAsync(object data, CancellationToken cancellationToken = default)
        => _client.PostAsync("api-lite/csds", data, cancellationToken: cancellationToken);

    /// <summary>Uploads the CSD of an issuer from the <c>.cer</c> and <c>.key</c> files the SAT delivers (<c>POST api-lite/csds</c>).</summary>
    /// <param name="rfc">The issuer's RFC.</param>
    /// <param name="certificatePath">The path of the <c>.cer</c> file.</param>
    /// <param name="privateKeyPath">The path of the <c>.key</c> file.</param>
    /// <param name="privateKeyPassword">The password of the private key.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public async Task<JsonNode?> CreateFromFilesAsync(string rfc, string certificatePath, string privateKeyPath, string privateKeyPassword, CancellationToken cancellationToken = default)
        => await CreateAsync(await ReadAsync(rfc, certificatePath, privateKeyPath, privateKeyPassword, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);

    /// <summary>Replaces the CSD of an issuer (<c>PUT api-lite/csds/{rfc}</c>).</summary>
    /// <param name="rfc">The issuer's RFC.</param>
    /// <param name="data">The payload: <c>Rfc</c>, <c>Certificate</c> and <c>PrivateKey</c> (base64) and <c>PrivateKeyPassword</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> UpdateAsync(string rfc, object data, CancellationToken cancellationToken = default)
        => _client.PutAsync(Path(rfc), data, cancellationToken: cancellationToken);

    /// <summary>Replaces the CSD of an issuer from the <c>.cer</c> and <c>.key</c> files the SAT delivers (<c>PUT api-lite/csds/{rfc}</c>).</summary>
    /// <param name="rfc">The issuer's RFC.</param>
    /// <param name="certificatePath">The path of the <c>.cer</c> file.</param>
    /// <param name="privateKeyPath">The path of the <c>.key</c> file.</param>
    /// <param name="privateKeyPassword">The password of the private key.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public async Task<JsonNode?> UpdateFromFilesAsync(string rfc, string certificatePath, string privateKeyPath, string privateKeyPassword, CancellationToken cancellationToken = default)
        => await UpdateAsync(rfc, await ReadAsync(rfc, certificatePath, privateKeyPath, privateKeyPassword, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);

    /// <summary>Deletes the CSD of an issuer (<c>DELETE api-lite/csds/{rfc}</c>).</summary>
    /// <param name="rfc">The issuer's RFC.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> DeleteAsync(string rfc, CancellationToken cancellationToken = default)
        => _client.DeleteAsync(Path(rfc), cancellationToken: cancellationToken);

    private static string Path(string rfc) => $"api-lite/csds/{Uri.EscapeDataString(rfc)}";

    private static async Task<JsonObject> ReadAsync(string rfc, string certificatePath, string privateKeyPath, string privateKeyPassword, CancellationToken cancellationToken)
        => new()
        {
            ["Rfc"] = rfc,
            ["Certificate"] = Convert.ToBase64String(await File.ReadAllBytesAsync(certificatePath, cancellationToken).ConfigureAwait(false)),
            ["PrivateKey"] = Convert.ToBase64String(await File.ReadAllBytesAsync(privateKeyPath, cancellationToken).ConfigureAwait(false)),
            ["PrivateKeyPassword"] = privateKeyPassword,
        };
}
