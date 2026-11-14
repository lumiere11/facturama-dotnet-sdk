using System.Text.Json.Nodes;

namespace Facturama.Resources;

/// <summary>CFDI 4.0 operations of the Multi-issuer API (<c>api-lite</c>).</summary>
public sealed class MultiissuerCfdis
{
    private readonly FacturamaClient _client;

    internal MultiissuerCfdis(FacturamaClient client) => _client = client;

    /// <summary>Creates (stamps) a CFDI 4.0 (<c>POST api-lite/3/cfdis</c>).</summary>
    /// <param name="cfdi">The CFDI payload, including the <c>Issuer</c> (an object, dictionary or <see cref="JsonNode"/> with the API's PascalCase properties).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The stamped CFDI.</returns>
    public Task<JsonNode?> CreateAsync(object cfdi, CancellationToken cancellationToken = default)
        => _client.PostAsync("api-lite/3/cfdis", cfdi, cancellationToken: cancellationToken);

    /// <summary>Gets one CFDI (<c>GET api-lite/cfdis/{id}</c>).</summary>
    /// <param name="id">The CFDI id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The CFDI.</returns>
    public Task<JsonNode?> FindAsync(string id, CancellationToken cancellationToken = default)
        => _client.GetAsync($"api-lite/cfdis/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);

    /// <summary>Lists CFDIs (<c>GET api-lite/cfdis</c>).</summary>
    /// <param name="filters">API query parameters (e.g. <c>keyword</c>), sent as given.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The matching CFDIs.</returns>
    public Task<JsonNode?> ListAsync(IEnumerable<KeyValuePair<string, object?>>? filters = null, CancellationToken cancellationToken = default)
        => _client.GetAsync("api-lite/cfdis", filters, cancellationToken);

    /// <summary>Downloads a CFDI (<c>GET cfdi/{format}/issuedLite/{id}</c>). The API returns the content base64 encoded in <c>Content</c>.</summary>
    /// <param name="id">The CFDI id.</param>
    /// <param name="format">One of <see cref="FileTypes"/>; PDF by default.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>An object with the file <c>Content</c> (base64) and metadata.</returns>
    public Task<JsonNode?> DownloadAsync(string id, string format = FileTypes.Pdf, CancellationToken cancellationToken = default)
        => _client.Cfdis.DownloadAsync(id, format, ReceiptTypes.IssuedLite, cancellationToken);

    /// <summary>Downloads a CFDI and returns the decoded file.</summary>
    /// <param name="id">The CFDI id.</param>
    /// <param name="format">One of <see cref="FileTypes"/>; PDF by default.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The bytes of the file.</returns>
    /// <exception cref="Exceptions.ResponseException">The response has no valid base64 <c>Content</c>.</exception>
    public Task<byte[]> DownloadBytesAsync(string id, string format = FileTypes.Pdf, CancellationToken cancellationToken = default)
        => _client.Cfdis.DownloadBytesAsync(id, format, ReceiptTypes.IssuedLite, cancellationToken);

    /// <summary>Downloads a CFDI and writes it to <paramref name="path"/> (an existing file is overwritten).</summary>
    /// <param name="id">The CFDI id.</param>
    /// <param name="path">The file to write.</param>
    /// <param name="format">One of <see cref="FileTypes"/>; PDF by default.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>A task that completes when the file is written.</returns>
    /// <exception cref="Exceptions.ResponseException">The response has no valid base64 <c>Content</c>.</exception>
    public Task DownloadToFileAsync(string id, string path, string format = FileTypes.Pdf, CancellationToken cancellationToken = default)
        => _client.Cfdis.DownloadToFileAsync(id, path, format, ReceiptTypes.IssuedLite, cancellationToken);

    /// <summary>Sends a CFDI by email (<c>POST Cfdi</c>).</summary>
    /// <param name="id">The CFDI id.</param>
    /// <param name="email">The recipient address.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> SendByEmailAsync(string id, string email, CancellationToken cancellationToken = default)
        => _client.Cfdis.SendByEmailAsync(id, email, ReceiptTypes.IssuedLite, cancellationToken);

    /// <summary>Cancels a CFDI (<c>DELETE api-lite/cfdis/{id}</c>).</summary>
    /// <param name="id">The CFDI id.</param>
    /// <param name="motive">The SAT cancellation motive (see <see cref="Model.Catalog.CancellationMotive"/>).</param>
    /// <param name="substitutionUuid">The UUID of the replacing CFDI; required for motive <c>01</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The cancellation acknowledgement.</returns>
    public Task<JsonNode?> CancelAsync(string id, string motive, string? substitutionUuid = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["type"] = ReceiptTypes.IssuedLite, ["motive"] = motive };
        if (substitutionUuid is not null)
        {
            query["uuidReplacement"] = substitutionUuid;
        }

        return _client.DeleteAsync($"api-lite/cfdis/{Uri.EscapeDataString(id)}", query, cancellationToken);
    }
}
