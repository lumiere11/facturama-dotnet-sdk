#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (one entry per SAT/Facturama code)

namespace Facturama.Model.Catalog;

/// <summary>
/// Static offline copy of the Facturama payment form catalog.
/// Source: <see href="https://www.api.facturama.com.mx/api/catalogs/PaymentForms"/>.
/// </summary>
public sealed class PaymentForm : ICatalog<CatalogEntry>
{
    private PaymentForm()
    {
    }

    /// <summary>Gets every payment form of the catalog.</summary>
    public static IReadOnlyList<CatalogEntry> Entries => AllEntries;

    public static readonly CatalogEntry Cash = new("Efectivo", "01");

    public static readonly CatalogEntry NominalCheck = new("Cheque nominativo", "02");

    public static readonly CatalogEntry Transfer = new("Transferencia electrónica de fondos", "03");

    public static readonly CatalogEntry CreditCard = new("Tarjeta de crédito", "04");

    public static readonly CatalogEntry DigitalWallet = new("Monedero electrónico", "05");

    public static readonly CatalogEntry DigitalMoney = new("Dinero electrónico", "06");

    public static readonly CatalogEntry GroceryCoupon = new("Vales de despensa", "08");

    public static readonly CatalogEntry Dation = new("Dación en pago", "12");

    public static readonly CatalogEntry Subrogation = new("Pago por subrogación", "13");

    public static readonly CatalogEntry Consignment = new("Pago por consignación", "14");

    public static readonly CatalogEntry Condonation = new("Condonación", "15");

    public static readonly CatalogEntry Compensation = new("Compensación", "17");

    public static readonly CatalogEntry Novation = new("Novación", "23");

    public static readonly CatalogEntry Confusion = new("Confusión", "24");

    public static readonly CatalogEntry DebtRemittance = new("Remisión de deuda", "25");

    public static readonly CatalogEntry Prescription = new("Prescripción o caducidad", "26");

    public static readonly CatalogEntry CreditorSatisfaction = new("A satisfacción del acreedor", "27");

    public static readonly CatalogEntry DebitCard = new("Tarjeta de débito", "28");

    public static readonly CatalogEntry ServiceCard = new("Tarjeta de servicios", "29");

    public static readonly CatalogEntry ApplicationOfAdvances = new("Aplicación de anticipos", "30");

    public static readonly CatalogEntry Intermediaries = new("Intermediarios", "31");

    public static readonly CatalogEntry ToBeDefined = new("Por definir", "99");

    private static readonly IReadOnlyList<CatalogEntry> AllEntries = Array.AsReadOnly<CatalogEntry>(
    [
        Cash,
        NominalCheck,
        Transfer,
        CreditCard,
        DigitalWallet,
        DigitalMoney,
        GroceryCoupon,
        Dation,
        Subrogation,
        Consignment,
        Condonation,
        Compensation,
        Novation,
        Confusion,
        DebtRemittance,
        Prescription,
        CreditorSatisfaction,
        DebitCard,
        ServiceCard,
        ApplicationOfAdvances,
        Intermediaries,
        ToBeDefined,
    ]);

    /// <inheritdoc />
    public static CatalogEntry FindByName(string name)
        => CatalogSearch.ByName(AllEntries, name, "Payment form");

    /// <inheritdoc />
    public static CatalogEntry FindByValue(string value)
        => CatalogSearch.ByValue(AllEntries, value, "Payment form");

    /// <inheritdoc />
    public static IEnumerable<CatalogEntry> FindAllByValue(string value)
        => CatalogSearch.AllByValue(AllEntries, value);
}
