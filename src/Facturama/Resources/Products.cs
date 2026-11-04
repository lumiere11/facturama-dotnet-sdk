using System.Text.Json.Nodes;

namespace Facturama.Resources;

/// <summary>CRUD for the account's products (<c>Product</c> / <c>Products</c> endpoints).</summary>
public sealed class Products
{
    private readonly FacturamaClient _client;

    internal Products(FacturamaClient client) => _client = client;

    /// <summary>Lists the products (<c>GET Products</c>).</summary>
    /// <param name="search">Optional text to filter by.</param>
    /// <param name="start">Zero-based index of the first item to return.</param>
    /// <param name="length">Maximum number of items to return.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The matching products.</returns>
    public Task<JsonNode?> ListAsync(string search = "", int start = 0, int length = 100, CancellationToken cancellationToken = default)
        => _client.GetAsync(
            "Products",
            new Dictionary<string, object?> { ["search"] = search, ["start"] = start, ["length"] = length },
            cancellationToken);

    /// <summary>Gets one item (<c>GET Product/{id}</c>).</summary>
    /// <param name="id">The item id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The item.</returns>
    public Task<JsonNode?> FindAsync(string id, CancellationToken cancellationToken = default)
        => _client.GetAsync($"Product/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);

    /// <summary>Creates an item (<c>POST Product</c>).</summary>
    /// <param name="data">The item payload (an object, dictionary or <see cref="JsonNode"/> with the API's PascalCase properties).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The created item.</returns>
    public Task<JsonNode?> CreateAsync(object data, CancellationToken cancellationToken = default)
        => _client.PostAsync("Product", data, cancellationToken: cancellationToken);

    /// <summary>Updates an item (<c>PUT Product/{id}</c>).</summary>
    /// <param name="id">The item id.</param>
    /// <param name="data">The item payload.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> UpdateAsync(string id, object data, CancellationToken cancellationToken = default)
        => _client.PutAsync($"Product/{Uri.EscapeDataString(id)}", data, cancellationToken: cancellationToken);

    /// <summary>Deletes an item (<c>DELETE Product/{id}</c>).</summary>
    /// <param name="id">The item id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The API response.</returns>
    public Task<JsonNode?> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => _client.DeleteAsync($"Product/{Uri.EscapeDataString(id)}", cancellationToken: cancellationToken);
}
