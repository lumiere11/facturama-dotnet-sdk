using System.Text.Json.Nodes;

namespace Facturama.Resources;

/// <summary>Builds the CSD payload from the <c>.cer</c> and <c>.key</c> files the SAT delivers.</summary>
internal static class CsdFiles
{
    public static async Task<JsonObject> ReadAsync(string? rfc, string certificatePath, string privateKeyPath, string privateKeyPassword, CancellationToken cancellationToken)
    {
        var csd = new JsonObject();
        if (rfc is not null)
        {
            csd["Rfc"] = rfc;
        }

        csd["Certificate"] = Convert.ToBase64String(await File.ReadAllBytesAsync(certificatePath, cancellationToken).ConfigureAwait(false));
        csd["PrivateKey"] = Convert.ToBase64String(await File.ReadAllBytesAsync(privateKeyPath, cancellationToken).ConfigureAwait(false));
        csd["PrivateKeyPassword"] = privateKeyPassword;

        return csd;
    }
}
