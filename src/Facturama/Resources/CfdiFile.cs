using System.Text.Json.Nodes;
using Facturama.Exceptions;

namespace Facturama.Resources;

/// <summary>Decodes the file the download endpoints return (base64 in <c>Content</c>).</summary>
internal static class CfdiFile
{
    public static byte[] Decode(JsonNode? file)
    {
        if (file is not JsonObject json || json["Content"] is not JsonValue value || !value.TryGetValue(out string? content))
        {
            throw new ResponseException("The download response has no \"Content\".");
        }

        try
        {
            return Convert.FromBase64String(content);
        }
        catch (FormatException e)
        {
            throw new ResponseException("The \"Content\" of the download response is not valid base64.", e);
        }
    }
}
