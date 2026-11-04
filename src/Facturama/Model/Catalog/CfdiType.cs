#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (one entry per SAT/Facturama code)

namespace Facturama.Model.Catalog;

/// <summary>
/// Static offline copy of the Facturama CFDI type catalog.
/// Source: <see href="https://www.api.facturama.com.mx/api/catalogs/CfdiTypes"/>.
/// </summary>
public sealed class CfdiType : ICatalog<CfdiTypeEntry>
{
    private CfdiType()
    {
    }

    /// <summary>Gets every CFDI type of the catalog.</summary>
    public static IReadOnlyList<CfdiTypeEntry> Entries => AllEntries;

    public static readonly CfdiTypeEntry Invoice = new("Factura", "I", 1);

    public static readonly CfdiTypeEntry CreditNote = new("Nota de Crédito", "E", 2);

    public static readonly CfdiTypeEntry Fee = new("Honorarios", "I", 3);

    public static readonly CfdiTypeEntry PorteLetter = new("Carta Porte", "T", 4);

    public static readonly CfdiTypeEntry RestaurantInvoice = new("Factura para Restaurantes", "I", 5);

    public static readonly CfdiTypeEntry GasStationInvoice = new("Factura para Gasolineras", "I", 6);

    public static readonly CfdiTypeEntry InvoiceWithTaxWithholding = new("Factura con Retención IVA", "I", 7);

    public static readonly CfdiTypeEntry PharmacyInvoice = new("Factura para Farmacias", "I", 8);

    public static readonly CfdiTypeEntry DeductibleReceipt = new("Recibo Deducible", "I", 9);

    public static readonly CfdiTypeEntry LeaseReceipt = new("Recibo de Arrendamiento", "I", 10);

    public static readonly CfdiTypeEntry SaleNote = new("Nota de Venta", "I", 11);

    public static readonly CfdiTypeEntry EstimationInvoice = new("Factura de Estimación", "I", 12);

    public static readonly CfdiTypeEntry SingleEstimationInvoice = new("Factura de Estimación Sencilla", "I", 13);

    public static readonly CfdiTypeEntry PaymentSupplement = new("Complemento de Pago", "P", 14);

    public static readonly CfdiTypeEntry HotelInvoice = new("Factura para Hoteles", "I", 15);

    public static readonly CfdiTypeEntry PayrollReceipt = new("Recibo de Nómina", "E", 16);

    public static readonly CfdiTypeEntry IepsInvoice = new("Factura con IEPS", "I", 17);

    public static readonly CfdiTypeEntry PaymentReceipt = new("Recibo de Pago", "P", 18);

    public static readonly CfdiTypeEntry SimplifiedEstimationInvoice = new("Factura de Estimación Simplificada", "I", 19);

    public static readonly CfdiTypeEntry PartialConstructionService = new("Servicio Parcial de Construcción", "I", 20);

    public static readonly CfdiTypeEntry IneInvoice = new("Factura INE", "I", 21);

    public static readonly CfdiTypeEntry DetailedInvoice = new("Factura Detallista", "I", 24);

    public static readonly CfdiTypeEntry PublicNotariesInvoice = new("Factura para Notarios Publicos", "I", 25);

    public static readonly CfdiTypeEntry ForeignTrade = new("Factura Comercio Exterior", "I", 26);

    public static readonly CfdiTypeEntry CreditNoteForeignTrade = new("Nota de Crédito Comercio Exterior", "E", 27);

    public static readonly CfdiTypeEntry PorteLetterForeignTrade = new("Carta Porte Comercio Exterior", "T", 28);

    private static readonly IReadOnlyList<CfdiTypeEntry> AllEntries = Array.AsReadOnly<CfdiTypeEntry>(
    [
        Invoice,
        CreditNote,
        Fee,
        PorteLetter,
        RestaurantInvoice,
        GasStationInvoice,
        InvoiceWithTaxWithholding,
        PharmacyInvoice,
        DeductibleReceipt,
        LeaseReceipt,
        SaleNote,
        EstimationInvoice,
        SingleEstimationInvoice,
        PaymentSupplement,
        HotelInvoice,
        PayrollReceipt,
        IepsInvoice,
        PaymentReceipt,
        SimplifiedEstimationInvoice,
        PartialConstructionService,
        IneInvoice,
        DetailedInvoice,
        PublicNotariesInvoice,
        ForeignTrade,
        CreditNoteForeignTrade,
        PorteLetterForeignTrade,
    ]);

    /// <inheritdoc />
    public static CfdiTypeEntry FindByName(string name)
        => CatalogSearch.ByName(AllEntries, name, "CFDI type");

    /// <inheritdoc />
    public static CfdiTypeEntry FindByValue(string value)
        => CatalogSearch.ByValue(AllEntries, value, "CFDI type");

    /// <inheritdoc />
    public static IEnumerable<CfdiTypeEntry> FindAllByValue(string value)
        => CatalogSearch.AllByValue(AllEntries, value);
}
