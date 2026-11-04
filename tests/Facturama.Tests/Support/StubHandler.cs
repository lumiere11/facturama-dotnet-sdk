using System.Net;
using System.Text;

namespace Facturama.Tests.Support;

/// <summary>A request as seen by the stub, captured before the framework disposes it.</summary>
public sealed record RecordedRequest(HttpMethod Method, Uri Uri, string? Body, string? Authorization, string UserAgent, string? ContentType);

/// <summary>An <see cref="HttpMessageHandler"/> that records requests and answers with canned responses (no network).</summary>
public sealed class StubHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

    public List<RecordedRequest> Requests { get; } = [];

    public RecordedRequest Last => Requests[^1];

    public static StubHandler Responding(HttpStatusCode status, string body)
        => new StubHandler().Enqueue(status, body);

    public StubHandler Enqueue(HttpStatusCode status, string body)
        => Enqueue(_ => new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") });

    public StubHandler Enqueue(Func<HttpRequestMessage, HttpResponseMessage> response)
    {
        _responses.Enqueue(response);

        return this;
    }

    public HttpClient CreateHttpClient() => new(this, disposeHandler: false);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add(new RecordedRequest(
            request.Method,
            request.RequestUri!,
            body,
            request.Headers.Authorization?.ToString(),
            request.Headers.UserAgent.ToString(),
            request.Content?.Headers.ContentType?.ToString()));

        return _responses.Count > 0
            ? _responses.Dequeue()(request)
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}", Encoding.UTF8, "application/json") };
    }
}
