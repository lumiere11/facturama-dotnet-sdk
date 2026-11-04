namespace Facturama.Exceptions;

/// <summary>A successful response carried a body that is not valid JSON.</summary>
public class ResponseException : FacturamaException
{
    /// <summary>Initializes a new instance of the <see cref="ResponseException"/> class.</summary>
    public ResponseException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ResponseException"/> class.</summary>
    /// <param name="message">The error message.</param>
    public ResponseException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ResponseException"/> class.</summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public ResponseException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
