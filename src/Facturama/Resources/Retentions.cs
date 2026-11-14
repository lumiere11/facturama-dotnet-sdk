using System.Text.Json.Nodes;

namespace Facturama.Resources;

/// <summary>Withholding CFDIs (CFDI de Retenciones e Información de Pagos; <c>retenciones</c> endpoints).</summary>
public sealed class Retentions
{
    private readonly FacturamaClient _client;

    internal Retentions(FacturamaClient client) => _client = client;

    /// <summary>Creates (stamps) a withholding CFDI (<c>POST 2/retenciones</c>).</summary>
    /// <param name="retention">The payload (an object, dictionary or <see cref="JsonNode"/> with the API's PascalCase properties).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The stamped withholding CFDI.</returns>
    public Task<JsonNode?> CreateAsync(object retention, CancellationToken cancellationToken = default)
        => _client.PostAsync("2/retenciones", retention, cancellationToken: cancellationToken);

    /// <summary>Gets one withholding CFDI (<c>GET Retenciones/{id}</c>).</summary>
    /// <param name="id">The withholding CFDI id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The withholding CFDI.</returns>
    public Task<JsonNode?> FindAsync(string id, CancellationToken cancellationToken = default)
        => _client.GetAsync($"Retenciones/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);

    /// <summary>Cancels a withholding CFDI (<c>DELETE Retenciones/{id}</c>).</summary>
    /// <param name="id">The withholding CFDI id.</param>
    /// <param name="motive">The SAT cancellation motive (see <see cref="Model.Catalog.CancellationMotive"/>).</param>
    /// <param name="substitutionUuid">The UUID of the replacing CFDI; required for motive <c>01</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The cancellation acknowledgement.</returns>
    public Task<JsonNode?> CancelAsync(string id, string motive, string? substitutionUuid = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["motive"] = motive };
        if (substitutionUuid is not null)
        {
            query["uuidReplacement"] = substitutionUuid;
        }

        return _client.DeleteAsync($"Retenciones/{Uri.EscapeDataString(id)}", query, cancellationToken);
    }
}
