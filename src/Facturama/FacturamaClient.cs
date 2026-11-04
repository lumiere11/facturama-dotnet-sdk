using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Facturama.Exceptions;
using Facturama.Resources;

namespace Facturama;

/// <summary>
/// Thin client for the Facturama REST API. It is the whole HTTP layer: <see cref="GetAsync"/>, <see cref="PostAsync"/>,
/// <see cref="PutAsync"/> and <see cref="DeleteAsync"/> take an API path such as <c>catalogs/CfdiUses</c> and return
/// the decoded JSON (<see cref="JsonNode"/>), or <see langword="null"/> for an empty body.
/// </summary>
/// <remarks>
/// <para>
/// The default environment is the <b>Sandbox</b> (<see cref="SandboxUrl"/>); to issue real CFDIs use
/// <see cref="FacturamaEnvironment.Production"/> or <see cref="SetApiUrl"/> with <see cref="ProductionUrl"/>.
/// </para>
/// <para>
/// Failures are translated to <see cref="RequestException"/> (carrying the API <c>Message</c>; validation messages
/// are chained as a <see cref="ModelException"/>) and <see cref="ResponseException"/> (unparseable success body).
/// All of them derive from <see cref="FacturamaException"/>.
/// </para>
/// <para>The instance is safe to share and reuse (register it as a singleton).</para>
/// </remarks>
public sealed class FacturamaClient : IDisposable
{
    /// <summary>Base URL of the test environment.</summary>
    public const string SandboxUrl = "https://apisandbox.facturama.mx";

    /// <summary>Base URL of the production environment.</summary>
    public const string ProductionUrl = "https://api.facturama.mx";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly HttpClient _http;
    private readonly bool _ownsHttpClient;
    private readonly AuthenticationHeaderValue? _authorization;
    private volatile string _baseUri;

    /// <summary>
    /// Initializes a client that creates its own <see cref="HttpClient"/> and targets the Sandbox.
    /// </summary>
    /// <param name="user">The Facturama account user.</param>
    /// <param name="password">The Facturama account password.</param>
    public FacturamaClient(string? user = null, string? password = null)
        : this(new FacturamaOptions { User = user, Password = password })
    {
    }

    /// <summary>
    /// Initializes a client from <paramref name="options"/>.
    /// </summary>
    /// <param name="options">The client settings.</param>
    /// <param name="httpClient">
    /// An existing <see cref="HttpClient"/> (e.g. from <c>IHttpClientFactory</c>, or one using a mock handler in tests).
    /// It is not disposed by this client and its timeout is left untouched, so
    /// <see cref="FacturamaOptions.ConnectTimeout"/> and <see cref="FacturamaOptions.Timeout"/> are ignored.
    /// When omitted, the client creates and owns its own.
    /// </param>
    public FacturamaClient(FacturamaOptions options, HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        _baseUri = NormalizeBaseUri(options.ResolveApiUrl());
        _authorization = CreateAuthorization(options.User, options.Password);

        if (httpClient is null)
        {
            _http = new HttpClient(new SocketsHttpHandler
            {
                ConnectTimeout = options.ConnectTimeout,
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            })
            {
                Timeout = options.Timeout,
            };
            _ownsHttpClient = true;
        }
        else
        {
            _http = httpClient;
        }
    }

    /// <summary>
    /// Initializes a client that uses an existing <see cref="HttpClient"/> and targets the Sandbox.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use. It is not disposed by this client.</param>
    /// <param name="user">The Facturama account user.</param>
    /// <param name="password">The Facturama account password.</param>
    public FacturamaClient(HttpClient httpClient, string? user = null, string? password = null)
        : this(new FacturamaOptions { User = user, Password = password }, httpClient ?? throw new ArgumentNullException(nameof(httpClient)))
    {
    }

    /// <summary>Gets the SDK version (the <c>Version</c> in <c>Directory.Build.props</c>).</summary>
    public static string Version { get; } = ReadVersion();

    /// <summary>Gets the <c>User-Agent</c> sent with every request.</summary>
    public static string UserAgent => "Facturama-DotNet-SDK-v" + Version;

    /// <summary>Gets the base URL the client sends requests to (no trailing slash).</summary>
    public string ApiUrl => _baseUri;

    /// <summary>Gets a value indicating whether this client targets the production API.</summary>
    public bool IsProduction => string.Equals(_baseUri, ProductionUrl, StringComparison.OrdinalIgnoreCase);

    /// <summary>Gets the CFDI 4.0 operations of the Web API.</summary>
    public Cfdis Cfdis => field ??= new Cfdis(this);

    /// <summary>Gets the CRUD operations for the account's clients.</summary>
    public Clients Clients => field ??= new Clients(this);

    /// <summary>Gets the CRUD operations for the account's products.</summary>
    public Products Products => field ??= new Products(this);

    /// <summary>Changes the base URL (e.g. <see cref="ProductionUrl"/>).</summary>
    /// <param name="baseUri">The new base URL; a trailing slash is ignored.</param>
    /// <returns>This client, to allow chaining.</returns>
    public FacturamaClient SetApiUrl(string baseUri)
    {
        _baseUri = NormalizeBaseUri(baseUri);

        return this;
    }

    /// <summary>Sends a GET request.</summary>
    /// <param name="path">The API path, e.g. <c>catalogs/CfdiUses</c>.</param>
    /// <param name="query">Optional query string parameters; <see langword="null"/> values are skipped.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The decoded JSON response, or <see langword="null"/> when the body is empty.</returns>
    public Task<JsonNode?> GetAsync(string path, IEnumerable<KeyValuePair<string, object?>>? query = null, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Get, path, null, query, cancellationToken);

    /// <summary>Sends a POST request with a JSON body.</summary>
    /// <param name="path">The API path, e.g. <c>3/cfdis</c>.</param>
    /// <param name="body">The payload, serialized with System.Text.Json (property names are kept as written, the API uses PascalCase). When <see langword="null"/> an empty JSON array (<c>[]</c>) is sent.</param>
    /// <param name="query">Optional query string parameters; <see langword="null"/> values are skipped.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The decoded JSON response, or <see langword="null"/> when the body is empty.</returns>
    public Task<JsonNode?> PostAsync(string path, object? body = null, IEnumerable<KeyValuePair<string, object?>>? query = null, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Post, path, body ?? Array.Empty<object>(), query, cancellationToken);

    /// <summary>Sends a PUT request with an optional JSON body.</summary>
    /// <param name="path">The API path, e.g. <c>Client/abc</c>.</param>
    /// <param name="body">The payload; when <see langword="null"/> no body is sent.</param>
    /// <param name="query">Optional query string parameters; <see langword="null"/> values are skipped.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The decoded JSON response, or <see langword="null"/> when the body is empty.</returns>
    public Task<JsonNode?> PutAsync(string path, object? body = null, IEnumerable<KeyValuePair<string, object?>>? query = null, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Put, path, body, query, cancellationToken);

    /// <summary>Sends a DELETE request.</summary>
    /// <param name="path">The API path, e.g. <c>Client/abc</c>.</param>
    /// <param name="query">Optional query string parameters; <see langword="null"/> values are skipped.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The decoded JSON response, or <see langword="null"/> when the body is empty.</returns>
    public Task<JsonNode?> DeleteAsync(string path, IEnumerable<KeyValuePair<string, object?>>? query = null, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Delete, path, null, query, cancellationToken);

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _http.Dispose();
        }
    }

    private static string NormalizeBaseUri(string baseUri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUri);

        return baseUri.TrimEnd('/');
    }

    private static AuthenticationHeaderValue? CreateAuthorization(string? user, string? password)
    {
        if (user is null && password is null)
        {
            return null;
        }

        return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{password}")));
    }

    private static string ReadVersion()
    {
        string? version = typeof(FacturamaClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (string.IsNullOrEmpty(version))
        {
            return "0.0.0";
        }

        int metadata = version.IndexOf('+', StringComparison.Ordinal);

        return metadata < 0 ? version : version[..metadata];
    }

    private static string FormatQueryValue(object value) => value switch
    {
        bool b => b ? "true" : "false",
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    private static string BuildQuery(IEnumerable<KeyValuePair<string, object?>>? query)
    {
        if (query is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach ((string key, object? value) in query)
        {
            if (value is null)
            {
                continue;
            }

            builder.Append(builder.Length == 0 ? '?' : '&')
                .Append(Uri.EscapeDataString(key))
                .Append('=')
                .Append(Uri.EscapeDataString(FormatQueryValue(value)));
        }

        return builder.ToString();
    }

    private static JsonNode? ParseSuccessBody(string content)
    {
        try
        {
            return JsonNode.Parse(content);
        }
        catch (JsonException e)
        {
            throw new ResponseException($"Response body could not be parsed since it doesn't contain a valid JSON structure, {e.Message}: {content}", e);
        }
    }

    private static RequestException CreateRequestException(HttpStatusCode statusCode, string? reasonPhrase, string content)
    {
        JsonNode? parsed = null;
        if (content.Length > 0)
        {
            try
            {
                parsed = JsonNode.Parse(content);
            }
            catch (JsonException)
            {
                // Not JSON: the raw body is used as the message below.
            }
        }

        if (parsed is JsonObject error && error["Message"] is JsonValue messageValue && messageValue.TryGetValue(out string? message))
        {
            ModelException? modelException = null;
            if (error["ModelState"] is JsonObject modelState)
            {
                var messages = new List<string>();
                foreach ((_, JsonNode? propertyMessages) in modelState)
                {
                    if (propertyMessages is JsonArray array)
                    {
                        messages.AddRange(array.Select(item => item?.GetValueKind() == JsonValueKind.String ? item.GetValue<string>() : item?.ToJsonString()).OfType<string>());
                    }
                }

                modelException = new ModelException(messages.Count > 0 ? string.Join("; ", messages) + "." : string.Empty);
            }

            return new RequestException(message, statusCode, modelException);
        }

        return new RequestException(content.Length > 0 ? content : $"{(int)statusCode} {reasonPhrase}".TrimEnd(), statusCode);
    }

    private async Task<JsonNode?> SendAsync(HttpMethod method, string path, object? body, IEnumerable<KeyValuePair<string, object?>>? query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(path);

        using var request = new HttpRequestMessage(method, $"{_baseUri}/{path.TrimStart('/')}{BuildQuery(query)}");
        request.Headers.UserAgent.ParseAdd(UserAgent);
        request.Headers.Authorization = _authorization;
        if (body is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        }

        HttpStatusCode statusCode;
        string? reasonPhrase;
        string content;
        try
        {
            using HttpResponseMessage response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            statusCode = response.StatusCode;
            reasonPhrase = response.ReasonPhrase;
            content = (await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)).Trim();
        }
        catch (HttpRequestException e)
        {
            throw new RequestException(e.Message, e.StatusCode, e);
        }
        catch (TaskCanceledException e) when (!cancellationToken.IsCancellationRequested)
        {
            throw new RequestException("The request timed out.", e);
        }

        if ((int)statusCode is < 200 or > 299)
        {
            throw CreateRequestException(statusCode, reasonPhrase, content);
        }

        return content.Length > 0 ? ParseSuccessBody(content) : null;
    }
}
