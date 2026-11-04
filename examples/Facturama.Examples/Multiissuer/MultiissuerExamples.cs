using System.Text.Json.Nodes;
using Facturama.Model.Catalog;

namespace Facturama.Examples.Multiissuer;

/// <summary>
/// Multi-issuer API: CFDIs stamped on behalf of several issuers, each with its own CSD
/// (digital seal certificate). These operations are NOT reflected in Facturama's web app.
/// </summary>
internal static class MultiissuerExamples
{
    /// <summary>CFDI 4.0 - Tipo Ingreso, with the issuer's data in the payload.</summary>
    public static Task<JsonNode?> CreateCfdiAsync(FacturamaClient client)
    {
        var cfdi = new
        {
            Serie = "A",
            NameId = "1",
            Folio = "999",
            CfdiType = "I",
            Currency = "MXN",
            PaymentForm = PaymentForm.FindByName("Efectivo").Value,
            PaymentMethod = PaymentMethod.SingleExhibition.Value,
            Exportation = "01",
            Date = (string?)null,
            ExpeditionPlace = "78140",
            OrderNumber = "TEST-001",
            PaymentAccountNumber = "6789",
            PaymentConditions = "Condiciones",
            Issuer = new
            {
                Rfc = "URE180429TM6",
                CfdiUse = CfdiUse.Payments.Value,
                Name = "UNIVERSIDAD ROBOTICA ESPAÑOLA",
                FiscalRegime = "601",
                TaxZipCode = "65000",
            },
            Receiver = new
            {
                Rfc = "OÑO120726RX3",
                CfdiUse = CfdiUse.GeneralExpenses.Value,
                Name = "ORGANICOS ÑAVEZ OSORIO",
                FiscalRegime = "601",
                TaxZipCode = "32040",
                Address = new
                {
                    Street = "Guadalcazar del receptor",
                    ExteriorNumber = "300",
                    InteriorNumber = "A",
                    Neighborhood = "Las lomas",
                    ZipCode = "32040",
                    Municipality = "San Luis Potosi",
                    State = States.SanLuisPotosi,
                    Country = "México",
                },
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
                    TaxObject = "02",
                    Taxes = new[]
                    {
                        new { Total = 16.0, Name = FederalTax.Iva.Name, Base = 100.0, Rate = 0.16, IsRetention = false },
                    },
                    Total = 116.0,
                },
            },
            LogoUrl = "https://www.ejemplos.co/wp-content/uploads/2015/11/Logo-Chanel.jpg",
            Observations = "Este es un ejemplo de observaciones",
            PaymentBankName = "BBVA",
        };

        return client.PostAsync("api-lite/3/cfdis", cfdi);
    }

    /// <summary>Downloads an issued (Lite) CFDI and saves it in the current directory.</summary>
    public static async Task<JsonNode?> DownloadAsync(FacturamaClient client, string id, string format)
    {
        // Multi-issuer CFDIs use the "issuedLite" type, which is not part of ReceiptTypes.
        JsonNode? file = await client.Cfdis.DownloadAsync(id, format, "issuedLite");
        string path = Path.GetFullPath($"cfdi-{id}.{format}");
        await File.WriteAllBytesAsync(path, Convert.FromBase64String(file!["Content"]!.GetValue<string>()));
        Console.WriteLine($"Saved {path}");

        return null;
    }

    /// <summary>
    /// Uploads the CSD of an issuer. Unlike the PHP example (which embeds the base64 inline), this reads the
    /// <c>.cer</c> and <c>.key</c> files the SAT delivers and base64-encodes them.
    /// </summary>
    public static async Task<JsonNode?> UploadCsdAsync(FacturamaClient client, string rfc, string certificatePath, string privateKeyPath, string privateKeyPassword)
    {
        var csd = new
        {
            Rfc = rfc,
            Certificate = Convert.ToBase64String(await File.ReadAllBytesAsync(certificatePath)),
            PrivateKey = Convert.ToBase64String(await File.ReadAllBytesAsync(privateKeyPath)),
            PrivateKeyPassword = privateKeyPassword,
        };

        return await client.PostAsync("api-lite/csds", csd);
    }
}
