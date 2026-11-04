namespace Facturama.Exceptions;

/// <summary>Base class of every exception thrown by the SDK.</summary>
public class FacturamaException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="FacturamaException"/> class.</summary>
    public FacturamaException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="FacturamaException"/> class.</summary>
    /// <param name="message">The error message.</param>
    public FacturamaException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="FacturamaException"/> class.</summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public FacturamaException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
