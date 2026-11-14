#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (one entry per SAT/Facturama code)

namespace Facturama.Model.Catalog;

/// <summary>
/// Static offline copy of the SAT cancellation motive catalog (<c>c_MotivoCancelacion</c>), the <c>motive</c> of the cancel operations.
/// </summary>
public sealed class CancellationMotive : ICatalog<CatalogEntry>
{
    private CancellationMotive()
    {
    }

    /// <summary>Gets every entry of the catalog.</summary>
    public static IReadOnlyList<CatalogEntry> Entries => AllEntries;

    public static readonly CatalogEntry ErrorsWithRelation = new("Comprobante emitido con errores con relación", "01");

    public static readonly CatalogEntry ErrorsWithoutRelation = new("Comprobante emitido con errores sin relación", "02");

    public static readonly CatalogEntry OperationNotCarriedOut = new("No se llevó a cabo la operación", "03");

    public static readonly CatalogEntry NominativeOperationInGlobalInvoice = new("Operación nominativa relacionada en una factura global", "04");


    private static readonly IReadOnlyList<CatalogEntry> AllEntries = Array.AsReadOnly<CatalogEntry>(
    [
        ErrorsWithRelation,
        ErrorsWithoutRelation,
        OperationNotCarriedOut,
        NominativeOperationInGlobalInvoice,
    ]);

    /// <inheritdoc />
    public static CatalogEntry FindByName(string name)
        => CatalogSearch.ByName(AllEntries, name, "Cancellation motive");

    /// <inheritdoc />
    public static CatalogEntry FindByValue(string value)
        => CatalogSearch.ByValue(AllEntries, value, "Cancellation motive");

    /// <inheritdoc />
    public static IEnumerable<CatalogEntry> FindAllByValue(string value)
        => CatalogSearch.AllByValue(AllEntries, value);
}
