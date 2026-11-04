namespace Facturama.Exceptions;

/// <summary>
/// The API rejected the payload. Its message joins the validation messages of the API's
/// <c>ModelState</c> (e.g. <c>"bad A; bad B."</c>). It is chained as the inner exception of a <see cref="RequestException"/>.
/// </summary>
public class ModelException : FacturamaException
{
    /// <summary>Initializes a new instance of the <see cref="ModelException"/> class.</summary>
    public ModelException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ModelException"/> class.</summary>
    /// <param name="message">The joined validation messages.</param>
    public ModelException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ModelException"/> class.</summary>
    /// <param name="message">The joined validation messages.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public ModelException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
