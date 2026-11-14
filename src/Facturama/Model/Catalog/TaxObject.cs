#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (one entry per SAT/Facturama code)

namespace Facturama.Model.Catalog;

/// <summary>
/// Static offline copy of the SAT tax object catalog (<c>c_ObjetoImp</c>), the <c>TaxObject</c> of a CFDI item.
/// </summary>
public sealed class TaxObject : ICatalog<CatalogEntry>
{
    private TaxObject()
    {
    }

    /// <summary>Gets every entry of the catalog.</summary>
    public static IReadOnlyList<CatalogEntry> Entries => AllEntries;

    public static readonly CatalogEntry NotSubject = new("No objeto de impuesto", "01");

    public static readonly CatalogEntry Subject = new("Sí objeto de impuesto", "02");

    public static readonly CatalogEntry SubjectWithoutBreakdown = new("Sí objeto del impuesto y no obligado al desglose", "03");

    public static readonly CatalogEntry SubjectNoTaxCaused = new("Sí objeto del impuesto y no causa impuesto", "04");


    public static readonly CatalogEntry SubjectVatCreditPodebi = new("Sí objeto del impuesto, IVA crédito PODEBI", "05");

    public static readonly CatalogEntry SubjectVatNotTransferred = new("Sí objeto del IVA, No traslado IVA", "06");

    public static readonly CatalogEntry VatNotTransferredIepsBreakdown = new("No traslado del IVA, Sí desglose IEPS", "07");

    public static readonly CatalogEntry VatNotTransferredNoIepsBreakdown = new("No traslado del IVA, No desglose IEPS", "08");

    private static readonly IReadOnlyList<CatalogEntry> AllEntries = Array.AsReadOnly<CatalogEntry>(
    [
        NotSubject,
        Subject,
        SubjectWithoutBreakdown,
        SubjectNoTaxCaused,
        SubjectVatCreditPodebi,
        SubjectVatNotTransferred,
        VatNotTransferredIepsBreakdown,
        VatNotTransferredNoIepsBreakdown,
    ]);

    /// <inheritdoc />
    public static CatalogEntry FindByName(string name)
        => CatalogSearch.ByName(AllEntries, name, "Tax object");

    /// <inheritdoc />
    public static CatalogEntry FindByValue(string value)
        => CatalogSearch.ByValue(AllEntries, value, "Tax object");

    /// <inheritdoc />
    public static IEnumerable<CatalogEntry> FindAllByValue(string value)
        => CatalogSearch.AllByValue(AllEntries, value);
}
