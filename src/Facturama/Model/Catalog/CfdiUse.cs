#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (one entry per SAT/Facturama code)

namespace Facturama.Model.Catalog;

/// <summary>
/// Static offline copy of the Facturama CFDI use catalog.
/// Source: <see href="https://www.api.facturama.com.mx/api/catalogs/CfdiUses"/>.
/// </summary>
public sealed class CfdiUse : ICatalog<CfdiUseEntry>
{
    private CfdiUse()
    {
    }

    /// <summary>Gets every CFDI use of the catalog.</summary>
    public static IReadOnlyList<CfdiUseEntry> Entries => AllEntries;

    public static readonly CfdiUseEntry GoodsAcquisition = new("Adquisición de mercancias", "G01", true, true);

    public static readonly CfdiUseEntry Returns = new("Devoluciones, descuentos o bonificaciones", "G02", true, true);

    public static readonly CfdiUseEntry GeneralExpenses = new("Gastos en general", "G03", true, true);

    public static readonly CfdiUseEntry Building = new("Construcciones", "I01", true, true);

    public static readonly CfdiUseEntry OfficeFurniture = new("Mobilario y equipo de oficina por inversiones", "I02", true, true);

    public static readonly CfdiUseEntry TransportEquipment = new("Equipo de transporte", "I03", true, true);

    public static readonly CfdiUseEntry ComputingEquipment = new("Equipo de computo y accesorios", "I04", true, true);

    public static readonly CfdiUseEntry Tooling = new("Dados, troqueles, moldes, matrices y herramental", "I05", true, true);

    public static readonly CfdiUseEntry PhoneCommunications = new("Comunicaciones telefónicas", "I06", true, true);

    public static readonly CfdiUseEntry SatellitalCommunication = new("Comunicaciones satelitales", "I07", true, true);

    public static readonly CfdiUseEntry OtherMachinery = new("Otra maquinaria y equipo", "I08", true, true);

    [Obsolete("`P01` was removed in CFDI 4.0; only valid for CFDI 3.3.")]
    public static readonly CfdiUseEntry ToBeDefined = new("Por definir", "P01", true, true);

    public static readonly CfdiUseEntry MedicalExpenses = new("Honorarios médicos, dentales y gastos hospitalarios.", "D01", false, true);

    public static readonly CfdiUseEntry MedicalExpensesByDisability = new("Gastos médicos por incapacidad o discapacidad", "D02", false, true);

    public static readonly CfdiUseEntry FuneralExpenses = new("Gastos funerales.", "D03", false, true);

    public static readonly CfdiUseEntry Donations = new("Donativos.", "D04", false, true);

    public static readonly CfdiUseEntry MortgageLoans = new("Intereses reales efectivamente pagados por créditos hipotecarios (casa habitación).", "D05", false, true);

    public static readonly CfdiUseEntry SarContributions = new("Aportaciones voluntarias al SAR.", "D06", false, true);

    public static readonly CfdiUseEntry MedicalInsurance = new("Primas por seguros de gastos médicos.", "D07", false, true);

    public static readonly CfdiUseEntry SchoolTransportationExpenses = new("Gastos de transportación escolar obligatoria.", "D08", false, true);

    public static readonly CfdiUseEntry PensionPlanDeposit = new("Depósitos en cuentas para el ahorro, primas que tengan como base planes de pensiones.", "D09", false, true);

    public static readonly CfdiUseEntry EducationalServicePayments = new("Pagos por servicios educativos (colegiaturas)", "D10", false, true);

    public static readonly CfdiUseEntry WithoutTaxEffects = new("Sin efectos fiscales", "S01", true, true);

    public static readonly CfdiUseEntry Payments = new("Pagos", "CP01", true, true);

    public static readonly CfdiUseEntry Payroll = new("Nómina", "CN01", false, true);

    private static readonly IReadOnlyList<CfdiUseEntry> AllEntries = Array.AsReadOnly<CfdiUseEntry>(
    [
#pragma warning disable CS0618 // The deprecated entries stay in the catalog on purpose.
        GoodsAcquisition,
        Returns,
        GeneralExpenses,
        Building,
        OfficeFurniture,
        TransportEquipment,
        ComputingEquipment,
        Tooling,
        PhoneCommunications,
        SatellitalCommunication,
        OtherMachinery,
        ToBeDefined,
        MedicalExpenses,
        MedicalExpensesByDisability,
        FuneralExpenses,
        Donations,
        MortgageLoans,
        SarContributions,
        MedicalInsurance,
        SchoolTransportationExpenses,
        PensionPlanDeposit,
        EducationalServicePayments,
        WithoutTaxEffects,
        Payments,
        Payroll,
#pragma warning restore CS0618
    ]);

    /// <inheritdoc />
    public static CfdiUseEntry FindByName(string name)
        => CatalogSearch.ByName(AllEntries, name, "CFDI use");

    /// <inheritdoc />
    public static CfdiUseEntry FindByValue(string value)
        => CatalogSearch.ByValue(AllEntries, value, "CFDI use");

    /// <inheritdoc />
    public static IEnumerable<CfdiUseEntry> FindAllByValue(string value)
        => CatalogSearch.AllByValue(AllEntries, value);
}
