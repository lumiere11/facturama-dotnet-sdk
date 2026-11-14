using System.Text.Json.Nodes;

namespace Facturama.Resources;

/// <summary>The invoice series of a branch office (<c>Serie/{idBranchOffice}</c> endpoints).</summary>
public sealed class Series
{
    private readonly FacturamaClient _client;

    internal Series(FacturamaClient client) => _client = client;

    /// <summary>Lists the series of a branch office (<c>GET Serie/{idBranchOffice}</c>).</summary>
    /// <param name="branchOfficeId">The branch office id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The series.</returns>
    public Task<JsonNode?> ListAsync(string branchOfficeId, CancellationToken cancellationToken = default)
        => _client.GetAsync(Path(branchOfficeId), cancellationToken: cancellationToken);

    /// <summary>Gets one series (<c>GET Serie/{idBranchOffice}/{name}</c>).</summary>
    /// <param name="branchOfficeId">The branch office id.</param>
    /// <param name="name">The series name.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The series.</returns>
    public Task<JsonNode?> FindAsync(string branchOfficeId, string name, CancellationToken cancellationToken = default)
        => _client.GetAsync(Path(branchOfficeId, name), cancellationToken: cancellationToken);

    /// <summary>Creates a series (<c>POST Serie/{idBranchOffice}</c>).</summary>
    /// <param name="branchOfficeId">The branch office id.</param>
    /// <param name="data">The series (<c>Name</c>, <c>Description</c>, <c>Folio</c>, ...).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The created series.</returns>
    public Task<JsonNode?> CreateAsync(string branchOfficeId, object data, CancellationToken cancellationToken = default)
        => _client.PostAsync(Path(branchOfficeId), data, cancellationToken: cancellationToken);

    /// <summary>Updates a series (<c>PUT Serie/{idBranchOffice}/{name}</c>).</summary>
    /// <param name="branchOfficeId">The branch office id.</param>
    /// <param name="name">The series name.</param>
    /// <param name="data">The new values.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> UpdateAsync(string branchOfficeId, string name, object data, CancellationToken cancellationToken = default)
        => _client.PutAsync(Path(branchOfficeId, name), data, cancellationToken: cancellationToken);

    /// <summary>Deletes a series (<c>DELETE Serie/{idBranchOffice}/{name}</c>).</summary>
    /// <param name="branchOfficeId">The branch office id.</param>
    /// <param name="name">The series name.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> DeleteAsync(string branchOfficeId, string name, CancellationToken cancellationToken = default)
        => _client.DeleteAsync(Path(branchOfficeId, name), cancellationToken: cancellationToken);

    private static string Path(string branchOfficeId) => $"Serie/{Uri.EscapeDataString(branchOfficeId)}";

    private static string Path(string branchOfficeId, string name) => $"{Path(branchOfficeId)}/{Uri.EscapeDataString(name)}";
}
