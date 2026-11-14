using System.Text.Json.Nodes;

namespace Facturama.Resources;

/// <summary>CRUD for the account's branch offices (<c>BranchOffice</c> endpoints).</summary>
public sealed class BranchOffices
{
    private readonly FacturamaClient _client;

    internal BranchOffices(FacturamaClient client) => _client = client;

    /// <summary>Lists the branch offices (<c>GET BranchOffice</c>).</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The branch offices.</returns>
    public Task<JsonNode?> ListAsync(CancellationToken cancellationToken = default)
        => _client.GetAsync("BranchOffice", cancellationToken: cancellationToken);

    /// <summary>Gets one branch office (<c>GET BranchOffice/{id}</c>).</summary>
    /// <param name="id">The branch office id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The branch office.</returns>
    public Task<JsonNode?> FindAsync(string id, CancellationToken cancellationToken = default)
        => _client.GetAsync(Path(id), cancellationToken: cancellationToken);

    /// <summary>Creates a branch office (<c>POST BranchOffice</c>).</summary>
    /// <param name="data">The branch office (<c>Name</c>, <c>Description</c>, <c>Address</c>, ...).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The created branch office.</returns>
    public Task<JsonNode?> CreateAsync(object data, CancellationToken cancellationToken = default)
        => _client.PostAsync("BranchOffice", data, cancellationToken: cancellationToken);

    /// <summary>Updates a branch office (<c>PUT BranchOffice/{id}</c>).</summary>
    /// <param name="id">The branch office id.</param>
    /// <param name="data">The new values.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> UpdateAsync(string id, object data, CancellationToken cancellationToken = default)
        => _client.PutAsync(Path(id), data, cancellationToken: cancellationToken);

    /// <summary>Deletes a branch office (<c>DELETE BranchOffice/{id}</c>).</summary>
    /// <param name="id">The branch office id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => _client.DeleteAsync(Path(id), cancellationToken: cancellationToken);

    private static string Path(string id) => $"BranchOffice/{Uri.EscapeDataString(id)}";
}
