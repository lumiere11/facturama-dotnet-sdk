#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (one entry per SAT/Facturama code)

namespace Facturama.Model.Catalog;

/// <summary>
/// Static offline copy of the SAT fiscal regime catalog (<c>c_RegimenFiscal</c>), the <c>FiscalRegime</c> of issuers and receivers.
/// </summary>
public sealed class FiscalRegime : ICatalog<CatalogEntry>
{
    private FiscalRegime()
    {
    }

    /// <summary>Gets every entry of the catalog.</summary>
    public static IReadOnlyList<CatalogEntry> Entries => AllEntries;

    public static readonly CatalogEntry GeneralLegalEntities = new("General de Ley Personas Morales", "601");

    public static readonly CatalogEntry NonProfitLegalEntities = new("Personas Morales con Fines no Lucrativos", "603");

    public static readonly CatalogEntry WagesAndSalaries = new("Sueldos y Salarios e Ingresos Asimilados a Salarios", "605");

    public static readonly CatalogEntry Leasing = new("Arrendamiento", "606");

    public static readonly CatalogEntry SaleOrAcquisitionOfGoods = new("Régimen de Enajenación o Adquisición de Bienes", "607");

    public static readonly CatalogEntry OtherIncome = new("Demás ingresos", "608");

    public static readonly CatalogEntry ForeignResidents = new("Residentes en el Extranjero sin Establecimiento Permanente en México", "610");

    public static readonly CatalogEntry Dividends = new("Ingresos por Dividendos (socios y accionistas)", "611");

    public static readonly CatalogEntry BusinessAndProfessionalActivities = new("Personas Físicas con Actividades Empresariales y Profesionales", "612");

    public static readonly CatalogEntry InterestIncome = new("Ingresos por intereses", "614");

    public static readonly CatalogEntry Prizes = new("Régimen de los ingresos por obtención de premios", "615");

    public static readonly CatalogEntry NoTaxObligations = new("Sin obligaciones fiscales", "616");

    public static readonly CatalogEntry ProductionCooperatives = new("Sociedades Cooperativas de Producción que optan por diferir sus ingresos", "620");

    public static readonly CatalogEntry FiscalIncorporation = new("Incorporación Fiscal", "621");

    public static readonly CatalogEntry AgricultureLivestockForestryAndFishing = new("Actividades Agrícolas, Ganaderas, Silvícolas y Pesqueras", "622");

    public static readonly CatalogEntry OptionalForGroupsOfCompanies = new("Opcional para Grupos de Sociedades", "623");

    public static readonly CatalogEntry Coordinated = new("Coordinados", "624");

    public static readonly CatalogEntry TechnologyPlatforms = new("Régimen de las Actividades Empresariales con ingresos a través de Plataformas Tecnológicas", "625");

    public static readonly CatalogEntry SimplifiedTrust = new("Régimen Simplificado de Confianza", "626");


    public static readonly CatalogEntry Hydrocarbons = new("Hidrocarburos", "628");

    public static readonly CatalogEntry PreferentialTaxRegimesAndMultinationals = new("De los Regímenes Fiscales Preferentes y de las Empresas Multinacionales", "629");

    public static readonly CatalogEntry StockExchangeShareSales = new("Enajenación de acciones en bolsa de valores", "630");

    private static readonly IReadOnlyList<CatalogEntry> AllEntries = Array.AsReadOnly<CatalogEntry>(
    [
        GeneralLegalEntities,
        NonProfitLegalEntities,
        WagesAndSalaries,
        Leasing,
        SaleOrAcquisitionOfGoods,
        OtherIncome,
        ForeignResidents,
        Dividends,
        BusinessAndProfessionalActivities,
        InterestIncome,
        Prizes,
        NoTaxObligations,
        ProductionCooperatives,
        FiscalIncorporation,
        AgricultureLivestockForestryAndFishing,
        OptionalForGroupsOfCompanies,
        Coordinated,
        TechnologyPlatforms,
        SimplifiedTrust,
        Hydrocarbons,
        PreferentialTaxRegimesAndMultinationals,
        StockExchangeShareSales,
    ]);

    /// <inheritdoc />
    public static CatalogEntry FindByName(string name)
        => CatalogSearch.ByName(AllEntries, name, "Fiscal regime");

    /// <inheritdoc />
    public static CatalogEntry FindByValue(string value)
        => CatalogSearch.ByValue(AllEntries, value, "Fiscal regime");

    /// <inheritdoc />
    public static IEnumerable<CatalogEntry> FindAllByValue(string value)
        => CatalogSearch.AllByValue(AllEntries, value);
}
