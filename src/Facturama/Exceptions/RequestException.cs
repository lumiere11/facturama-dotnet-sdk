using System.Net;

namespace Facturama.Exceptions;

/// <summary>
/// The request failed: the API answered with an error status (the message is the API's <c>Message</c> when present),
/// or the HTTP call itself failed (network error, timeout).
/// </summary>
public class RequestException : FacturamaException
{
    /// <summary>Initializes a new instance of the <see cref="RequestException"/> class.</summary>
    public RequestException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RequestException"/> class.</summary>
    /// <param name="message">The error message.</param>
    public RequestException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RequestException"/> class.</summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public RequestException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RequestException"/> class.</summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code of the response, if any.</param>
    /// <param name="innerException">The exception that caused this one (a <see cref="ModelException"/> for validation errors).</param>
    public RequestException(string message, HttpStatusCode? statusCode, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>Gets the HTTP status code of the response, or <see langword="null"/> when no response was received.</summary>
    public HttpStatusCode? StatusCode { get; }
}
