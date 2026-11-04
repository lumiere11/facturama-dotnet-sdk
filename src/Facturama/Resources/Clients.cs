using System.Text.Json.Nodes;

namespace Facturama.Resources;

/// <summary>CRUD for the account's clients (<c>Client</c> / <c>Clients</c> endpoints).</summary>
public sealed class Clients
{
    private readonly FacturamaClient _client;

    internal Clients(FacturamaClient client) => _client = client;

    /// <summary>Lists the clients (<c>GET Clients</c>).</summary>
    /// <param name="search">Optional text to filter by.</param>
    /// <param name="start">Zero-based index of the first item to return.</param>
    /// <param name="length">Maximum number of items to return.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The matching clients.</returns>
    public Task<JsonNode?> ListAsync(string search = "", int start = 0, int length = 100, CancellationToken cancellationToken = default)
        => _client.GetAsync(
            "Clients",
            new Dictionary<string, object?> { ["search"] = search, ["start"] = start, ["length"] = length },
            cancellationToken);

    /// <summary>Gets one item (<c>GET Client/{id}</c>).</summary>
    /// <param name="id">The item id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The item.</returns>
    public Task<JsonNode?> FindAsync(string id, CancellationToken cancellationToken = default)
        => _client.GetAsync($"Client/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);

    /// <summary>Creates an item (<c>POST Client</c>).</summary>
    /// <param name="data">The item payload (an object, dictionary or <see cref="JsonNode"/> with the API's PascalCase properties).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The created item.</returns>
    public Task<JsonNode?> CreateAsync(object data, CancellationToken cancellationToken = default)
        => _client.PostAsync("Client", data, cancellationToken: cancellationToken);

    /// <summary>Updates an item (<c>PUT Client/{id}</c>).</summary>
    /// <param name="id">The item id.</param>
    /// <param name="data">The item payload.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> UpdateAsync(string id, object data, CancellationToken cancellationToken = default)
        => _client.PutAsync($"Client/{Uri.EscapeDataString(id)}", data, cancellationToken: cancellationToken);

    /// <summary>Deletes an item (<c>DELETE Client/{id}</c>).</summary>
    /// <param name="id">The item id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => _client.DeleteAsync($"Client/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);
}
