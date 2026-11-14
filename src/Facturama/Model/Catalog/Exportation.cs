#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (one entry per SAT/Facturama code)

namespace Facturama.Model.Catalog;

/// <summary>
/// Static offline copy of the SAT exportation catalog (<c>c_Exportacion</c>), the <c>Exportation</c> of a CFDI.
/// </summary>
public sealed class Exportation : ICatalog<CatalogEntry>
{
    private Exportation()
    {
    }

    /// <summary>Gets every entry of the catalog.</summary>
    public static IReadOnlyList<CatalogEntry> Entries => AllEntries;

    public static readonly CatalogEntry NotApplicable = new("No aplica", "01");

    public static readonly CatalogEntry DefinitiveA1 = new("Definitiva con clave A1", "02");

    public static readonly CatalogEntry Temporary = new("Temporal", "03");

    public static readonly CatalogEntry DefinitiveOtherThanA1 = new("Definitiva con clave distinta a A1 o cuando no existe enajenación en términos del CFF", "04");


    private static readonly IReadOnlyList<CatalogEntry> AllEntries = Array.AsReadOnly<CatalogEntry>(
    [
        NotApplicable,
        DefinitiveA1,
        Temporary,
        DefinitiveOtherThanA1,
    ]);

    /// <inheritdoc />
    public static CatalogEntry FindByName(string name)
        => CatalogSearch.ByName(AllEntries, name, "Exportation");

    /// <inheritdoc />
    public static CatalogEntry FindByValue(string value)
        => CatalogSearch.ByValue(AllEntries, value, "Exportation");

    /// <inheritdoc />
    public static IEnumerable<CatalogEntry> FindAllByValue(string value)
        => CatalogSearch.AllByValue(AllEntries, value);
}
