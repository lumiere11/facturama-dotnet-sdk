namespace Facturama;

/// <summary>The <c>type</c> of CFDI the API operations work with.</summary>
public static class ReceiptTypes
{
    /// <summary>CFDIs issued by the account.</summary>
    public const string Issued = "issued";

    /// <summary>CFDIs received by the account.</summary>
    public const string Received = "received";

    /// <summary>Payroll CFDIs.</summary>
    public const string Payroll = "payroll";
}
