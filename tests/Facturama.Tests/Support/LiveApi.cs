using System.Text.Json.Nodes;

namespace Facturama.Tests.Support;

/// <summary>
/// A test that calls the real Facturama Sandbox. It needs a valid account in the
/// <c>FACTURAMA_SANDBOX_USER</c> / <c>FACTURAMA_SANDBOX_PASSWORD</c> environment variables and is skipped without them.
/// Live tests carry the <c>Category=Live</c> trait so the offline run can exclude them.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class LiveFactAttribute : FactAttribute
{
    public const string UserVariable = "FACTURAMA_SANDBOX_USER";
    public const string PasswordVariable = "FACTURAMA_SANDBOX_PASSWORD";

    public LiveFactAttribute()
    {
        if (string.IsNullOrEmpty(LiveApi.User) || string.IsNullOrEmpty(LiveApi.Password))
        {
            Skip = $"Set {UserVariable} and {PasswordVariable} to run the Sandbox tests.";
        }
    }
}

internal static class LiveApi
{
    public static string? User { get; } = Environment.GetEnvironmentVariable(LiveFactAttribute.UserVariable);

    public static string? Password { get; } = Environment.GetEnvironmentVariable(LiveFactAttribute.PasswordVariable);

    public static FacturamaClient CreateClient()
        => new FacturamaClient(User, Password).SetApiUrl(FacturamaClient.SandboxUrl);

    public static async Task<JsonArray> GetCatalogAsync(FacturamaClient client, string path, string? keyword = null)
    {
        Dictionary<string, object?>? query = keyword is null ? null : new() { ["keyword"] = keyword };
        var result = await client.GetAsync(path, query);

        return Assert.IsType<JsonArray>(result);
    }
}
