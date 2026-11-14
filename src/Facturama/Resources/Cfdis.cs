using System.Text.Json.Nodes;

namespace Facturama.Resources;

/// <summary>CFDI 4.0 operations of the Web API (single issuer).</summary>
public sealed class Cfdis
{
    private readonly FacturamaClient _client;

    internal Cfdis(FacturamaClient client) => _client = client;

    /// <summary>Creates (stamps) a CFDI 4.0 (<c>POST 3/cfdis</c>).</summary>
    /// <param name="cfdi">The CFDI payload (an object, dictionary or <see cref="JsonNode"/> with the API's PascalCase properties).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The stamped CFDI.</returns>
    public Task<JsonNode?> CreateAsync(object cfdi, CancellationToken cancellationToken = default)
        => _client.PostAsync("3/cfdis", cfdi, cancellationToken: cancellationToken);

    /// <summary>Lists CFDIs (<c>GET Cfdi</c>).</summary>
    /// <param name="filters">API query parameters (<c>type</c>, <c>folioStart</c>, <c>rfc</c>, <c>page</c>, ...). <c>type</c> defaults to <see cref="ReceiptTypes.Issued"/>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The matching CFDIs.</returns>
    public Task<JsonNode?> ListAsync(IEnumerable<KeyValuePair<string, object?>>? filters = null, CancellationToken cancellationToken = default)
    {
        var query = new List<KeyValuePair<string, object?>>(filters ?? []);
        if (!query.Exists(pair => pair.Key == "type"))
        {
            query.Add(new KeyValuePair<string, object?>("type", ReceiptTypes.Issued));
        }

        return _client.GetAsync("Cfdi", query, cancellationToken);
    }

    /// <summary>Downloads a CFDI (<c>GET cfdi/{format}/{type}/{id}</c>). The API returns the content base64 encoded in <c>Content</c>.</summary>
    /// <param name="id">The CFDI id.</param>
    /// <param name="format">One of <see cref="FileTypes"/>; PDF by default.</param>
    /// <param name="type">One of <see cref="ReceiptTypes"/>; issued by default.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>An object with the file <c>Content</c> (base64) and metadata.</returns>
    public Task<JsonNode?> DownloadAsync(string id, string format = FileTypes.Pdf, string type = ReceiptTypes.Issued, CancellationToken cancellationToken = default)
        => _client.GetAsync($"cfdi/{Uri.EscapeDataString(format)}/{Uri.EscapeDataString(type)}/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);

    /// <summary>Downloads a CFDI and returns the decoded file.</summary>
    /// <param name="id">The CFDI id.</param>
    /// <param name="format">One of <see cref="FileTypes"/>; PDF by default.</param>
    /// <param name="type">One of <see cref="ReceiptTypes"/>; issued by default.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The bytes of the file.</returns>
    /// <exception cref="Exceptions.ResponseException">The response has no valid base64 <c>Content</c>.</exception>
    public async Task<byte[]> DownloadBytesAsync(string id, string format = FileTypes.Pdf, string type = ReceiptTypes.Issued, CancellationToken cancellationToken = default)
        => CfdiFile.Decode(await DownloadAsync(id, format, type, cancellationToken).ConfigureAwait(false));

    /// <summary>Downloads a CFDI and writes it to <paramref name="path"/> (an existing file is overwritten).</summary>
    /// <param name="id">The CFDI id.</param>
    /// <param name="path">The file to write.</param>
    /// <param name="format">One of <see cref="FileTypes"/>; PDF by default.</param>
    /// <param name="type">One of <see cref="ReceiptTypes"/>; issued by default.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>A task that completes when the file is written.</returns>
    /// <exception cref="Exceptions.ResponseException">The response has no valid base64 <c>Content</c>.</exception>
    public async Task DownloadToFileAsync(string id, string path, string format = FileTypes.Pdf, string type = ReceiptTypes.Issued, CancellationToken cancellationToken = default)
    {
        byte[] content = await DownloadBytesAsync(id, format, type, cancellationToken).ConfigureAwait(false);
        await File.WriteAllBytesAsync(path, content, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Sends a CFDI by email (<c>POST Cfdi</c>).</summary>
    /// <param name="id">The CFDI id.</param>
    /// <param name="email">The recipient address.</param>
    /// <param name="type">One of <see cref="ReceiptTypes"/>; issued by default.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> SendByEmailAsync(string id, string email, string type = ReceiptTypes.Issued, CancellationToken cancellationToken = default)
        => _client.PostAsync(
            "Cfdi",
            null,
            new Dictionary<string, object?> { ["cfdiType"] = type, ["cfdiId"] = id, ["email"] = email },
            cancellationToken);

    /// <summary>Cancels a CFDI (<c>DELETE Cfdi/{id}</c>).</summary>
    /// <param name="id">The CFDI id.</param>
    /// <param name="motive">The SAT cancellation motive (<c>01</c> to <c>04</c>).</param>
    /// <param name="type">One of <see cref="ReceiptTypes"/>; issued by default.</param>
    /// <param name="substitutionUuid">The UUID of the replacing CFDI; required for motive <c>01</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The cancellation acknowledgement.</returns>
    public Task<JsonNode?> CancelAsync(string id, string motive, string type = ReceiptTypes.Issued, string? substitutionUuid = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["type"] = type, ["motive"] = motive };
        if (substitutionUuid is not null)
        {
            query["uuidReplacement"] = substitutionUuid;
        }

        return _client.DeleteAsync($"Cfdi/{Uri.EscapeDataString(id)}", query, cancellationToken);
    }
}
