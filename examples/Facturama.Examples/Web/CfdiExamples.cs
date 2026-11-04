using System.Text.Json.Nodes;
using Facturama.Model.Catalog;

namespace Facturama.Examples.Web;

/// <summary>CFDI 4.0 examples of the Web API (single issuer).</summary>
internal static class CfdiExamples
{
    /// <summary>CFDI 4.0 - Tipo Ingreso. The catalogs give the codes a name instead of a magic string.</summary>
    public static Task<JsonNode?> CreateInvoiceAsync(FacturamaClient client)
    {
        var cfdi = new
        {
            Serie = "A",
            Folio = "100",
            Date = "2022-03-30",
            PaymentForm = PaymentForm.FindByName("Efectivo").Value,
            PaymentConditions = "CREDITO A SIETE DIAS",
            Currency = "MXN",
            CfdiType = "I",
            PaymentMethod = PaymentMethod.SingleExhibition.Value,
            ExpeditionPlace = "78140",
            Exportation = "01",
            Receiver = new
            {
                Rfc = "URE180429TM6",
                CfdiUse = CfdiUse.GeneralExpenses.Value,
                Name = "UNIVERSIDAD ROBOTICA ESPAÑOLA",
                FiscalRegime = "603",
                TaxZipCode = "65000",
                Address = new
                {
                    Street = "Guadalcazar del receptor",
                    ExteriorNumber = "300",
                    InteriorNumber = "A",
                    Neighborhood = "Las lomas",
                    ZipCode = "65000",
                    Municipality = "San Luis Potosi",
                    State = States.SanLuisPotosi,
                    Country = "México",
                },
            },
            Items = new[] { Item("Estudios de viabilidad") },
        };

        return client.Cfdis.CreateAsync(cfdi);
    }

    /// <summary>CFDI 4.0 - Tipo Ingreso como Factura a Público en General.</summary>
    public static Task<JsonNode?> CreatePublicInvoiceAsync(FacturamaClient client)
    {
        var cfdi = new
        {
            Currency = "MXN",
            ExpeditionPlace = "78140",
            Exportation = "01",
            PaymentConditions = "CREDITO A SIETE DIAS",
            Folio = "100",
            CfdiType = "I",
            PaymentForm = PaymentForm.FindByName("Transferencia electrónica de fondos").Value,
            PaymentMethod = PaymentMethod.SingleExhibition.Value,
            Receiver = new
            {
                Rfc = "XAXX010101000",
                Name = "PúBLICO EN GENERAL",
                CfdiUse = CfdiUse.WithoutTaxEffects.Value,
                TaxZipCode = "78140",
                FiscalRegime = "616",
            },
            Items = new[] { Item("Estudios de laboratorio") },
        };

        return client.Cfdis.CreateAsync(cfdi);
    }

    /// <summary>CFDI 4.0 - Factura Global.</summary>
    public static Task<JsonNode?> CreateGlobalInvoiceAsync(FacturamaClient client)
    {
        var cfdi = new
        {
            Currency = "MXN",
            ExpeditionPlace = "78140",
            PaymentConditions = "CREDITO A SIETE DIAS",
            Folio = "100",
            CfdiType = "I",
            PaymentForm = PaymentForm.FindByName("Transferencia electrónica de fondos").Value,
            PaymentMethod = PaymentMethod.SingleExhibition.Value,
            Exportation = "01",
            GlobalInformation = new { Periodicity = "04", Months = "04", Year = "2022" },
            Receiver = new
            {
                Rfc = "XAXX010101000",
                Name = "PUBLICO EN GENERAL",
                CfdiUse = CfdiUse.WithoutTaxEffects.Value,
                TaxZipCode = "78140",
                FiscalRegime = "616",
            },
            Items = new[] { Item("Estudios de laboratorio") },
        };

        return client.Cfdis.CreateAsync(cfdi);
    }

    /// <summary>CFDI 4.0 - Factura con complemento a cuenta de terceros.</summary>
    public static Task<JsonNode?> CreateThirdPartyInvoiceAsync(FacturamaClient client)
    {
        var cfdi = new
        {
            Serie = "A",
            Folio = "100",
            Date = (string?)null,
            PaymentForm = PaymentForm.FindByName("Efectivo").Value,
            PaymentConditions = "CREDITO A SIETE DIAS",
            Currency = "MXN",
            CfdiType = "I",
            PaymentMethod = PaymentMethod.SingleExhibition.Value,
            ExpeditionPlace = "78140",
            Exportation = "01",
            Receiver = new
            {
                Rfc = "URE180429TM6",
                CfdiUse = CfdiUse.GeneralExpenses.Value,
                Name = "UNIVERSIDAD ROBOTICA ESPAÑOLA",
                FiscalRegime = "603",
                TaxZipCode = "65000",
            },
            Items = new[]
            {
                new
                {
                    ProductCode = "10101504",
                    IdentificationNumber = "EDL",
                    Description = "Estudios de viabilidad",
                    Unit = "NO APLICA",
                    UnitCode = "MTS",
                    UnitPrice = 50.0,
                    Quantity = 2.0,
                    Subtotal = 100.0,
                    ThirdPartyAccount = new
                    {
                        Rfc = "CACX7605101P8",
                        Name = "XOCHILT CASAS CHAVEZ",
                        FiscalRegime = "616",
                        TaxZipCode = "10740",
                    },
                    PropertyTaxIDNumber = new[] { "123456789" },
                    TaxObject = "02",
                    Taxes = new[] { Iva(100.0) },
                    Total = 116.0,
                },
            },
        };

        return client.Cfdis.CreateAsync(cfdi);
    }

    /// <summary>Lists the issued CFDIs (the SDK adds <c>type=issued</c> by default).</summary>
    public static Task<JsonNode?> ListAsync(FacturamaClient client, string page)
        => client.Cfdis.ListAsync(new Dictionary<string, object?> { ["status"] = "all", ["page"] = page });

    /// <summary>Downloads a CFDI and saves it next to the executable. The API returns the file base64 encoded in <c>Content</c>.</summary>
    public static async Task<JsonNode?> DownloadAsync(FacturamaClient client, string id, string format)
    {
        JsonNode? file = await client.Cfdis.DownloadAsync(id, format);
        string path = Path.GetFullPath($"cfdi-{id}.{format}");
        await File.WriteAllBytesAsync(path, Convert.FromBase64String(file!["Content"]!.GetValue<string>()));
        Console.WriteLine($"Saved {path}");

        return null;
    }

    public static Task<JsonNode?> SendByEmailAsync(FacturamaClient client, string id, string email)
        => client.Cfdis.SendByEmailAsync(id, email);

    /// <summary>Cancels a CFDI. Motive <c>01</c> also needs the UUID of the replacing CFDI.</summary>
    public static Task<JsonNode?> CancelAsync(FacturamaClient client, string id, string motive)
        => client.Cfdis.CancelAsync(id, motive);

    private static object Item(string description) => new
    {
        ProductCode = "10101504",
        IdentificationNumber = "EDL",
        Description = description,
        Unit = "NO APLICA",
        UnitCode = "MTS",
        UnitPrice = 50.0,
        Quantity = 2.0,
        Subtotal = 100.0,
        TaxObject = "02",
        Taxes = new[] { Iva(100.0) },
        Total = 116.0,
    };

    private static object Iva(double baseAmount) => new
    {
        Total = baseAmount * 0.16,
        Name = FederalTax.Iva.Name,
        Base = baseAmount,
        Rate = 0.16,
        IsRetention = false,
    };
}
