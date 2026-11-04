using Facturama.Model.Catalog;

namespace Facturama.Tests.Model.Catalog;

/// <summary>Offline tests for the local catalogs: no network access required.</summary>
public sealed class CatalogsOfflineTests
{
    private sealed record CatalogUnderTest(
        IReadOnlyList<CatalogEntry> Entries,
        Func<string, CatalogEntry> FindByName,
        Func<string, CatalogEntry> FindByValue,
        Func<string, IEnumerable<CatalogEntry>> FindAllByValue);

    private static readonly Dictionary<string, CatalogUnderTest> Catalogs = new()
    {
        [nameof(CfdiType)] = Of<CfdiType, CfdiTypeEntry>(),
        [nameof(CfdiUse)] = Of<CfdiUse, CfdiUseEntry>(),
        [nameof(FederalTax)] = Of<FederalTax, CatalogEntry>(),
        [nameof(PaymentForm)] = Of<PaymentForm, CatalogEntry>(),
        [nameof(PaymentMethod)] = Of<PaymentMethod, CatalogEntry>(),
    };

    public static TheoryData<string> CatalogNames => [.. Catalogs.Keys];

    [Theory]
    [MemberData(nameof(CatalogNames))]
    public void CatalogIsNotEmptyAndEntriesAreWellFormed(string catalog)
    {
        Assert.NotEmpty(Catalogs[catalog].Entries);
        foreach (CatalogEntry entry in Catalogs[catalog].Entries)
        {
            Assert.False(string.IsNullOrEmpty(entry.Name));
            Assert.False(string.IsNullOrEmpty(entry.Value));
        }
    }

    [Theory]
    [MemberData(nameof(CatalogNames))]
    public void NamesAreUnique(string catalog)
    {
        string[] names = [.. Catalogs[catalog].Entries.Select(entry => entry.Name)];

        Assert.Equal(names.Length, names.Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [MemberData(nameof(CatalogNames))]
    public void FindByNameAndValueRoundTrip(string catalog)
    {
        CatalogUnderTest under = Catalogs[catalog];
        foreach (CatalogEntry entry in under.Entries)
        {
            Assert.Same(entry, under.FindByName(entry.Name));

            // Values are not unique in every catalog (e.g. FederalTax, CfdiType), so only the value is compared.
            Assert.Equal(entry.Value, under.FindByValue(entry.Value).Value);
            Assert.Contains(entry, under.FindAllByValue(entry.Value));
        }
    }

    [Theory]
    [MemberData(nameof(CatalogNames))]
    public void UnknownNameThrows(string catalog)
        => Assert.Throws<ArgumentException>(() => Catalogs[catalog].FindByName("__not-found__"));

    [Theory]
    [MemberData(nameof(CatalogNames))]
    public void UnknownValueThrowsOrYieldsNothing(string catalog)
    {
        Assert.Throws<ArgumentException>(() => Catalogs[catalog].FindByValue("__not-found__"));
        Assert.Empty(Catalogs[catalog].FindAllByValue("__not-found__"));
    }

    [Theory]
    [MemberData(nameof(CatalogNames))]
    public void EntriesCannotBeModifiedThroughTheList(string catalog)
        => Assert.False(Catalogs[catalog].Entries is CatalogEntry[] or List<CatalogEntry>);

    [Fact]
    public void CfdiUseValuesAreUnique()
    {
        string[] values = [.. CfdiUse.Entries.Select(entry => entry.Value)];

        Assert.Equal(values.Length, values.Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [InlineData("S01", "Sin efectos fiscales")]
    [InlineData("CP01", "Pagos")]
    [InlineData("CN01", "Nómina")]
    public void Cfdi40UsesAreAvailable(string value, string name)
        => Assert.Equal(name, CfdiUse.FindByValue(value).Name);

    [Fact]
    public void PayrollUseIsNaturalPersonsOnly()
    {
        CfdiUseEntry use = CfdiUse.FindByValue("CN01");

        Assert.True(use.Natural);
        Assert.False(use.Moral);
        Assert.Same(CfdiUse.Payroll, use);
    }

    [Fact]
    public void ToBeDefinedUseIsObsoleteButStillResolvable()
    {
        var field = typeof(CfdiUse).GetField("ToBeDefined");

        Assert.NotNull(field);
        Assert.NotNull(Attribute.GetCustomAttribute(field, typeof(ObsoleteAttribute)));
        Assert.Equal("P01", CfdiUse.FindByValue("P01").Value);
    }

    [Fact]
    public void CfdiTypeValuesAreShared()
    {
        CfdiTypeEntry[] invoices = [.. CfdiType.FindAllByValue("I")];

        Assert.True(invoices.Length > 1);
        Assert.Contains(CfdiType.Invoice, invoices);
        Assert.Equal(1, CfdiType.Invoice.NameId);
        Assert.Same(CfdiType.Invoice, CfdiType.FindByValue("I"));
    }

    [Fact]
    public void FederalTaxValuesAreSharedBetweenTaxAndRetention()
    {
        Assert.Equal(FederalTax.Iva.Value, FederalTax.IvaRet.Value);
        Assert.Same(FederalTax.Iva, FederalTax.FindByValue("002"));
        Assert.Equal(2, FederalTax.FindAllByValue("002").Count());
    }

    [Fact]
    public void PaymentCatalogsExposeTheCommonCodes()
    {
        Assert.Equal("01", PaymentForm.FindByName("Efectivo").Value);
        Assert.Equal("PUE", PaymentMethod.SingleExhibition.Value);
        Assert.Equal("PPD", PaymentMethod.Deferred.Value);
    }

    [Fact]
    public void StatesUseTheApiSpelling()
    {
        Assert.Equal("CIUDAD DE MEXICO", States.CiudadDeMexico);
        Assert.Equal("VERACRUZ DE IGNACIO DE LA LLAVE", States.VeracruzDeIgnacioDeLaLlave);
    }

    private static CatalogUnderTest Of<TCatalog, TEntry>()
        where TCatalog : ICatalog<TEntry>
        where TEntry : CatalogEntry
        => new(
            TCatalog.Entries,
            TCatalog.FindByName,
            TCatalog.FindByValue,
            value => TCatalog.FindAllByValue(value));
}
