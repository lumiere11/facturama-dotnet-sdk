#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (one entry per SAT/Facturama code)

namespace Facturama.Model.Catalog;

/// <summary>
/// Static offline copy of the SAT periodicity catalog (<c>c_Periodicidad</c>), the <c>Periodicity</c> of a global invoice.
/// </summary>
public sealed class Periodicity : ICatalog<CatalogEntry>
{
    private Periodicity()
    {
    }

    /// <summary>Gets every entry of the catalog.</summary>
    public static IReadOnlyList<CatalogEntry> Entries => AllEntries;

    public static readonly CatalogEntry Daily = new("Diario", "01");

    public static readonly CatalogEntry Weekly = new("Semanal", "02");

    public static readonly CatalogEntry Biweekly = new("Quincenal", "03");

    public static readonly CatalogEntry Monthly = new("Mensual", "04");

    public static readonly CatalogEntry Bimonthly = new("Bimestral", "05");


    private static readonly IReadOnlyList<CatalogEntry> AllEntries = Array.AsReadOnly<CatalogEntry>(
    [
        Daily,
        Weekly,
        Biweekly,
        Monthly,
        Bimonthly,
    ]);

    /// <inheritdoc />
    public static CatalogEntry FindByName(string name)
        => CatalogSearch.ByName(AllEntries, name, "Periodicity");

    /// <inheritdoc />
    public static CatalogEntry FindByValue(string value)
        => CatalogSearch.ByValue(AllEntries, value, "Periodicity");

    /// <inheritdoc />
    public static IEnumerable<CatalogEntry> FindAllByValue(string value)
        => CatalogSearch.AllByValue(AllEntries, value);
}
