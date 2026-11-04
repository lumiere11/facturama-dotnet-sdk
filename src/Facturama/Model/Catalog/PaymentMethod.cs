#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (one entry per SAT/Facturama code)

namespace Facturama.Model.Catalog;

/// <summary>
/// Static offline copy of the Facturama payment method catalog.
/// Source: <see href="https://www.api.facturama.com.mx/api/catalogs/PaymentMethods"/>.
/// </summary>
public sealed class PaymentMethod : ICatalog<CatalogEntry>
{
    private PaymentMethod()
    {
    }

    /// <summary>Gets every payment method of the catalog.</summary>
    public static IReadOnlyList<CatalogEntry> Entries => AllEntries;

    public static readonly CatalogEntry Initial = new("Pago inicial y parcialidades", "PIP");

    public static readonly CatalogEntry Deferred = new("Pago en parcialidades ó diferido", "PPD");

    public static readonly CatalogEntry SingleExhibition = new("Pago en una sola exhibición", "PUE");

    private static readonly IReadOnlyList<CatalogEntry> AllEntries = Array.AsReadOnly<CatalogEntry>(
    [
        Initial,
        Deferred,
        SingleExhibition,
    ]);

    /// <inheritdoc />
    public static CatalogEntry FindByName(string name)
        => CatalogSearch.ByName(AllEntries, name, "Payment method");

    /// <inheritdoc />
    public static CatalogEntry FindByValue(string value)
        => CatalogSearch.ByValue(AllEntries, value, "Payment method");

    /// <inheritdoc />
    public static IEnumerable<CatalogEntry> FindAllByValue(string value)
        => CatalogSearch.AllByValue(AllEntries, value);
}
