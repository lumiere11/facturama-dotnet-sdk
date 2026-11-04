#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (one entry per SAT/Facturama code)

namespace Facturama.Model.Catalog;

/// <summary>
/// Static offline copy of the Facturama federal tax catalog.
/// Source: <see href="https://www.api.facturama.com.mx/api/catalogs/FederalTaxes"/>.
/// </summary>
public sealed class FederalTax : ICatalog<CatalogEntry>
{
    private FederalTax()
    {
    }

    /// <summary>Gets every federal tax of the catalog.</summary>
    public static IReadOnlyList<CatalogEntry> Entries => AllEntries;

    public static readonly CatalogEntry Ieps = new("IEPS", "003");

    public static readonly CatalogEntry IepsRet = new("IEPS RET", "003");

    public static readonly CatalogEntry Isr = new("ISR", "001");

    public static readonly CatalogEntry Iva = new("IVA", "002");

    public static readonly CatalogEntry IvaRet = new("IVA RET", "002");

    private static readonly IReadOnlyList<CatalogEntry> AllEntries = Array.AsReadOnly<CatalogEntry>(
    [
        Ieps,
        IepsRet,
        Isr,
        Iva,
        IvaRet,
    ]);

    /// <inheritdoc />
    public static CatalogEntry FindByName(string name)
        => CatalogSearch.ByName(AllEntries, name, "Federal tax");

    /// <inheritdoc />
    public static CatalogEntry FindByValue(string value)
        => CatalogSearch.ByValue(AllEntries, value, "Federal tax");

    /// <inheritdoc />
    public static IEnumerable<CatalogEntry> FindAllByValue(string value)
        => CatalogSearch.AllByValue(AllEntries, value);
}
