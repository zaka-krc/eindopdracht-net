using ClosedXML.Excel;
using SuntoryManagementSystem.Models.Constants;
using SuntoryManagementSystem_Models.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SuntoryManagementSystem.Services
{
    // ReportService - Genereert Excel rapporten met ClosedXML
    // Alle rapporten worden opgeslagen in: Documenten\Suntory Reports\
    public class ReportService
    {
        private readonly SuntoryDbContext _context;
        private readonly string _reportsDirectory;

        public ReportService(SuntoryDbContext context)
        {
            _context = context;
            _reportsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Suntory Reports"
            );

            // Zorg ervoor dat de directory bestaat
            if (!Directory.Exists(_reportsDirectory))
            {
                Directory.CreateDirectory(_reportsDirectory);
            }
        }

        // Genereert een Inventarisrapport met huidige voorraadniveaus en hun waarden
        public string GenerateInventoryReport()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Inventaris");
                SetupWorksheet(worksheet);

                // Titel
                worksheet.Cell(1, 1).Value = "INVENTARISRAPPORT";
                worksheet.Range(1, 1, 1, 7).Merge();
                FormatTitle(worksheet.Cell(1, 1));

                // Datum
                worksheet.Cell(2, 1).Value = $"Gegenereerd op: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";
                worksheet.Cell(2, 1).Style.Font.Italic = true;

                // Koppen
                int row = 4;
                var headers = new[] { "Product", "SKU", "Categorie", "Huidige Voorraad", "Min. Voorraad", "Inkoopprijs", "Totale Waarde" };
                for (int col = 1; col <= headers.Length; col++)
                {
                    var cell = worksheet.Cell(row, col);
                    cell.Value = headers[col - 1];
                    FormatHeader(cell);
                }

                // Actieve producten ophalen
                var products = _context.Products
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.ProductName)
                    .ToList();

                // Gegevensrijen
                row = 5;
                decimal totalInventoryValue = 0;

                foreach (var product in products)
                {
                    worksheet.Cell(row, 1).Value = product.ProductName;
                    worksheet.Cell(row, 2).Value = product.SKU;
                    worksheet.Cell(row, 3).Value = product.Category;
                    worksheet.Cell(row, 4).Value = product.StockQuantity;
                    worksheet.Cell(row, 5).Value = product.MinimumStock;
                    worksheet.Cell(row, 6).Value = product.PurchasePrice;
                    worksheet.Cell(row, 6).Style.NumberFormat.Format = "€#,##0.00";

                    decimal totalValue = product.StockQuantity * product.PurchasePrice;
                    totalInventoryValue += totalValue;

                    worksheet.Cell(row, 7).Value = totalValue;
                    worksheet.Cell(row, 7).Style.NumberFormat.Format = "€#,##0.00";

                    // Lage voorraad markeren
                    if (product.StockQuantity < product.MinimumStock)
                    {
                        worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.Orange;
                    }

                    row++;
                }

                // Samenvatting
                row += 1;
                worksheet.Cell(row, 6).Value = "TOTALE WAARDE:";
                worksheet.Cell(row, 6).Style.Font.Bold = true;
                worksheet.Cell(row, 7).Value = totalInventoryValue;
                worksheet.Cell(row, 7).Style.NumberFormat.Format = "€#,##0.00";
                worksheet.Cell(row, 7).Style.Font.Bold = true;

                // Kolombreedtes aanpassen
                worksheet.Column(1).Width = 25;
                worksheet.Column(2).Width = 15;
                worksheet.Column(3).Width = 15;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 15;
                worksheet.Column(7).Width = 15;

                return SaveWorkbook(workbook, "Inventaris_Rapport");
            }
        }

        // Genereert een Voorraadbewegingen Rapport met toevoegingen en verwijderingen
        public string GenerateStockMovementReport()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Voorraadbewegingen");
                SetupWorksheet(worksheet);

                // Titel
                worksheet.Cell(1, 1).Value = "VOORRAADBEWEGINGEN RAPPORT";
                worksheet.Range(1, 1, 1, 9).Merge();
                FormatTitle(worksheet.Cell(1, 1));

                // Datum
                worksheet.Cell(2, 1).Value = $"Gegenereerd op: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";
                worksheet.Cell(2, 1).Style.Font.Italic = true;

                // Koppen
                int row = 4;
                var headers = new[] { "Datum", "Product", "Type", "Hoeveelheid", "Voor", "Na", "Reden", "Aangepast door" };
                for (int col = 1; col <= headers.Length; col++)
                {
                    var cell = worksheet.Cell(row, col);
                    cell.Value = headers[col - 1];
                    FormatHeader(cell);
                }

                // Voorraadcorrecties ophalen
                var adjustments = _context.StockAdjustments
                    .Where(sa => !sa.IsDeleted)
                    .OrderByDescending(sa => sa.AdjustmentDate)
                    .ToList();

                // Gegevensrijen
                row = 5;
                foreach (var adjustment in adjustments)
                {
                    var product = adjustment.Product;
                    if (product == null) continue;

                    worksheet.Cell(row, 1).Value = adjustment.AdjustmentDate;
                    worksheet.Cell(row, 1).Style.NumberFormat.Format = "dd-MM-yyyy HH:mm";

                    worksheet.Cell(row, 2).Value = product.ProductName;
                    worksheet.Cell(row, 3).Value = adjustment.AdjustmentType;
                    worksheet.Cell(row, 4).Value = adjustment.QuantityChange;
                    worksheet.Cell(row, 5).Value = adjustment.PreviousQuantity;
                    worksheet.Cell(row, 6).Value = adjustment.NewQuantity;
                    worksheet.Cell(row, 7).Value = adjustment.Reason;
                    worksheet.Cell(row, 8).Value = adjustment.AdjustedBy;

                    // Aanpassingstypen kleurcoderen
                    if (adjustment.AdjustmentType == StockAdjustmentConstants.Types.Addition)
                    {
                        worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    }
                    else if (adjustment.AdjustmentType == StockAdjustmentConstants.Types.Removal)
                    {
                        worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.Red;
                    }
                    else if (adjustment.AdjustmentType == StockAdjustmentConstants.Types.Damage || 
                             adjustment.AdjustmentType == StockAdjustmentConstants.Types.Theft)
                    {
                        worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.Orange;
                    }

                    row++;
                }

                // Kolombreedtes aanpassen
                worksheet.Column(1).Width = 18;
                worksheet.Column(2).Width = 25;
                worksheet.Column(3).Width = 12;
                worksheet.Column(4).Width = 12;
                worksheet.Column(5).Width = 8;
                worksheet.Column(6).Width = 8;
                worksheet.Column(7).Width = 30;
                worksheet.Column(8).Width = 18;

                return SaveWorkbook(workbook, "Voorraadbewegingen_Rapport");
            }
        }

        // Genereert een Leveringen Rapport met inkomende en uitgaande leveringen
        public string GenerateDeliveryReport()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Leveringen");
                SetupWorksheet(worksheet);

                // Titel
                worksheet.Cell(1, 1).Value = "LEVERINGEN RAPPORT";
                worksheet.Range(1, 1, 1, 8).Merge();
                FormatTitle(worksheet.Cell(1, 1));

                // Datum
                worksheet.Cell(2, 1).Value = $"Gegenereerd op: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";
                worksheet.Cell(2, 1).Style.Font.Italic = true;

                // Koppen
                int row = 4;
                var headers = new[] { "Referentie", "Type", "Partner", "Verwachte Datum", "Werkelijke Datum", "Status", "Bedrag", "Verwerkt" };
                for (int col = 1; col <= headers.Length; col++)
                {
                    var cell = worksheet.Cell(row, col);
                    cell.Value = headers[col - 1];
                    FormatHeader(cell);
                }

                // Leveringen ophalen
                var deliveries = _context.Deliveries
                    .Where(d => !d.IsDeleted)
                    .OrderByDescending(d => d.ExpectedDeliveryDate)
                    .ToList();

                // Gegevensrijen
                row = 5;
                decimal totalAmount = 0;

                foreach (var delivery in deliveries)
                {
                    worksheet.Cell(row, 1).Value = delivery.ReferenceNumber;
                    worksheet.Cell(row, 2).Value = delivery.DeliveryType == DeliveryConstants.Types.Incoming ? "Inkomend" : "Uitgaand";
                    
                    // Partnerinformatie
                    string partnerName = "";
                    if (delivery.DeliveryType == DeliveryConstants.Types.Incoming && delivery.Supplier != null)
                    {
                        partnerName = delivery.Supplier.SupplierName;
                    }
                    else if (delivery.DeliveryType == DeliveryConstants.Types.Outgoing && delivery.Customer != null)
                    {
                        partnerName = delivery.Customer.CustomerName;
                    }
                    worksheet.Cell(row, 3).Value = partnerName;

                    worksheet.Cell(row, 4).Value = delivery.ExpectedDeliveryDate;
                    worksheet.Cell(row, 4).Style.NumberFormat.Format = "dd-MM-yyyy";

                    if (delivery.ActualDeliveryDate.HasValue)
                    {
                        worksheet.Cell(row, 5).Value = delivery.ActualDeliveryDate.Value;
                        worksheet.Cell(row, 5).Style.NumberFormat.Format = "dd-MM-yyyy";
                    }

                    worksheet.Cell(row, 6).Value = delivery.Status;
                    worksheet.Cell(row, 7).Value = delivery.TotalAmount;
                    worksheet.Cell(row, 7).Style.NumberFormat.Format = "€#,##0.00";
                    totalAmount += delivery.TotalAmount;

                    worksheet.Cell(row, 8).Value = delivery.IsProcessed ? "Ja" : "Nee";

                    // Status kleurcoderen
                    if (delivery.Status == DeliveryConstants.Status.Delivered)
                    {
                        worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    }
                    else if (delivery.Status == DeliveryConstants.Status.Cancelled)
                    {
                        worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.Red;
                    }
                    else
                    {
                        worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.Yellow;
                    }

                    row++;
                }

                // Samenvatting
                row += 1;
                worksheet.Cell(row, 6).Value = "TOTAAL BEDRAG:";
                worksheet.Cell(row, 6).Style.Font.Bold = true;
                worksheet.Cell(row, 7).Value = totalAmount;
                worksheet.Cell(row, 7).Style.NumberFormat.Format = "€#,##0.00";
                worksheet.Cell(row, 7).Style.Font.Bold = true;

                // Kolombreedtes aanpassen
                worksheet.Column(1).Width = 15;
                worksheet.Column(2).Width = 12;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 16;
                worksheet.Column(5).Width = 16;
                worksheet.Column(6).Width = 12;
                worksheet.Column(7).Width = 12;
                worksheet.Column(8).Width = 10;

                return SaveWorkbook(workbook, "Leveringen_Rapport");
            }
        }

        // Genereert een Voorraadwaarschuwingen Rapport met lage en kritische voorraad items
        public string GenerateStockAlertReport()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Waarschuwingen");
                SetupWorksheet(worksheet);

                // Titel
                worksheet.Cell(1, 1).Value = "VOORRAADWAARSCHUWINGEN RAPPORT";
                worksheet.Range(1, 1, 1, 7).Merge();
                FormatTitle(worksheet.Cell(1, 1));

                // Datum
                worksheet.Cell(2, 1).Value = $"Gegenereerd op: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";
                worksheet.Cell(2, 1).Style.Font.Italic = true;

                // Lage voorraad producten ophalen
                var lowStockProducts = _context.Products
                    .Where(p => !p.IsDeleted && p.StockQuantity < p.MinimumStock)
                    .OrderBy(p => p.StockQuantity / (decimal)p.MinimumStock)
                    .ToList();

                // Actieve voorraadwaarschuwingen ophalen
                var stockAlerts = _context.StockAlerts
                    .Where(sa => !sa.IsDeleted && sa.Status == StockAlertConstants.Status.Active)
                    .OrderBy(sa => sa.CreatedDate)
                    .ToList();

                // Sectie 1: Kritiek Lage Voorraad
                int row = 4;
                worksheet.Cell(row, 1).Value = "KRITIEKE LAGE VOORRAAD";
                worksheet.Cell(row, 1).Style.Font.Bold = true;

                row++;
                var headers = new[] { "Product", "SKU", "Huidige Voorraad", "Min. Voorraad", "Tekort", "Leverancier" };
                for (int col = 1; col <= headers.Length; col++)
                {
                    var cell = worksheet.Cell(row, col);
                    cell.Value = headers[col - 1];
                    FormatHeader(cell);
                }

                row++;
                var criticalProducts = lowStockProducts
                    .Where(p => p.StockQuantity == 0 || (p.StockQuantity < p.MinimumStock && p.StockQuantity < p.MinimumStock / 2))
                    .ToList();

                foreach (var product in criticalProducts)
                {
                    worksheet.Cell(row, 1).Value = product.ProductName;
                    worksheet.Cell(row, 2).Value = product.SKU;
                    worksheet.Cell(row, 3).Value = product.StockQuantity;
                    worksheet.Cell(row, 4).Value = product.MinimumStock;
                    worksheet.Cell(row, 5).Value = product.MinimumStock - product.StockQuantity;
                    worksheet.Cell(row, 6).Value = product.Supplier?.SupplierName ?? "N/A";
                    worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.Red;
                    row++;
                }

                // Sectie 2: Lage Voorraad
                row += 1;
                worksheet.Cell(row, 1).Value = "LAGE VOORRAAD";
                worksheet.Cell(row, 1).Style.Font.Bold = true;

                row++;
                for (int col = 1; col <= headers.Length; col++)
                {
                    var cell = worksheet.Cell(row, col);
                    cell.Value = headers[col - 1];
                    FormatHeader(cell);
                }

                row++;
                var warningProducts = lowStockProducts.Except(criticalProducts).ToList();
                foreach (var product in warningProducts)
                {
                    worksheet.Cell(row, 1).Value = product.ProductName;
                    worksheet.Cell(row, 2).Value = product.SKU;
                    worksheet.Cell(row, 3).Value = product.StockQuantity;
                    worksheet.Cell(row, 4).Value = product.MinimumStock;
                    worksheet.Cell(row, 5).Value = product.MinimumStock - product.StockQuantity;
                    worksheet.Cell(row, 6).Value = product.Supplier?.SupplierName ?? "N/A";
                    worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.Orange;
                    row++;
                }

                // Sectie 3: Actieve Waarschuwingen
                row += 2;
                worksheet.Cell(row, 1).Value = "ACTIEVE WAARSCHUWINGEN";
                worksheet.Cell(row, 1).Style.Font.Bold = true;

                row++;
                var alertHeaders = new[] { "Product", "Type", "Status", "Aangemaakt", "Opmerkingen" };
                for (int col = 1; col <= alertHeaders.Length; col++)
                {
                    var cell = worksheet.Cell(row, col);
                    cell.Value = alertHeaders[col - 1];
                    FormatHeader(cell);
                }

                row++;
                foreach (var alert in stockAlerts)
                {
                    var product = alert.Product;
                    if (product == null) continue;

                    worksheet.Cell(row, 1).Value = product.ProductName;
                    worksheet.Cell(row, 2).Value = alert.AlertType;
                    worksheet.Cell(row, 3).Value = alert.Status;
                    worksheet.Cell(row, 4).Value = alert.CreatedDate;
                    worksheet.Cell(row, 4).Style.NumberFormat.Format = "dd-MM-yyyy HH:mm";
                    worksheet.Cell(row, 5).Value = alert.Notes;
                    row++;
                }

                // Kolombreedtes aanpassen
                worksheet.Column(1).Width = 25;
                worksheet.Column(2).Width = 12;
                worksheet.Column(3).Width = 15;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 12;
                worksheet.Column(6).Width = 18;

                return SaveWorkbook(workbook, "Waarschuwingen_Rapport");
            }
        }

        // Genereert een Financieel Rapport met analyse van inkoopwaarden, verkoopwaarden en marges
        public string GenerateFinancialReport()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Financieel");
                SetupWorksheet(worksheet);

                // Titel
                worksheet.Cell(1, 1).Value = "FINANCIEEL RAPPORT";
                worksheet.Range(1, 1, 1, 8).Merge();
                FormatTitle(worksheet.Cell(1, 1));

                // Datum
                worksheet.Cell(2, 1).Value = $"Gegenereerd op: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";
                worksheet.Cell(2, 1).Style.Font.Italic = true;

                // Koppen
                int row = 4;
                var headers = new[] { "Product", "SKU", "Huidige Voorraad", "Inkoopprijs", "Verkoopprijs", "Inkoopwaarde", "Verkoopwaarde", "Marge %" };
                for (int col = 1; col <= headers.Length; col++)
                {
                    var cell = worksheet.Cell(row, col);
                    cell.Value = headers[col - 1];
                    FormatHeader(cell);
                }

                // Actieve producten ophalen
                var products = _context.Products
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.ProductName)
                    .ToList();

                // Gegevensrijen
                row = 5;
                decimal totalPurchaseValue = 0;
                decimal totalSellingValue = 0;

                foreach (var product in products)
                {
                    decimal purchaseValue = product.StockQuantity * product.PurchasePrice;
                    decimal sellingValue = product.StockQuantity * product.SellingPrice;
                    decimal margin = product.PurchasePrice > 0 
                        ? ((product.SellingPrice - product.PurchasePrice) / product.PurchasePrice) * 100 
                        : 0;

                    totalPurchaseValue += purchaseValue;
                    totalSellingValue += sellingValue;

                    worksheet.Cell(row, 1).Value = product.ProductName;
                    worksheet.Cell(row, 2).Value = product.SKU;
                    worksheet.Cell(row, 3).Value = product.StockQuantity;
                    worksheet.Cell(row, 4).Value = product.PurchasePrice;
                    worksheet.Cell(row, 4).Style.NumberFormat.Format = "€#,##0.00";
                    worksheet.Cell(row, 5).Value = product.SellingPrice;
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "€#,##0.00";
                    worksheet.Cell(row, 6).Value = purchaseValue;
                    worksheet.Cell(row, 6).Style.NumberFormat.Format = "€#,##0.00";
                    worksheet.Cell(row, 7).Value = sellingValue;
                    worksheet.Cell(row, 7).Style.NumberFormat.Format = "€#,##0.00";
                    worksheet.Cell(row, 8).Value = margin;
                    worksheet.Cell(row, 8).Style.NumberFormat.Format = "0.00%";

                    row++;
                }

                // Samenvatting
                row += 1;
                decimal potentialProfit = totalSellingValue - totalPurchaseValue;
                decimal profitMargin = totalPurchaseValue > 0 
                    ? (potentialProfit / totalPurchaseValue) * 100 
                    : 0;

                worksheet.Cell(row, 5).Value = "TOTALE INKOOPWAARDE:";
                worksheet.Cell(row, 5).Style.Font.Bold = true;
                worksheet.Cell(row, 6).Value = totalPurchaseValue;
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "€#,##0.00";
                worksheet.Cell(row, 6).Style.Font.Bold = true;

                row++;
                worksheet.Cell(row, 5).Value = "TOTALE VERKOOPWAARDE:";
                worksheet.Cell(row, 5).Style.Font.Bold = true;
                worksheet.Cell(row, 6).Value = totalSellingValue;
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "€#,##0.00";
                worksheet.Cell(row, 6).Style.Font.Bold = true;

                row++;
                worksheet.Cell(row, 5).Value = "POTENTIËLE WINST:";
                worksheet.Cell(row, 5).Style.Font.Bold = true;
                worksheet.Cell(row, 6).Value = potentialProfit;
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "€#,##0.00";
                worksheet.Cell(row, 6).Style.Font.Bold = true;
                worksheet.Cell(row, 6).Style.Fill.BackgroundColor = potentialProfit >= 0 ? XLColor.LightGreen : XLColor.Red;

                row++;
                worksheet.Cell(row, 5).Value = "WINSTMARGE:";
                worksheet.Cell(row, 5).Style.Font.Bold = true;
                worksheet.Cell(row, 6).Value = profitMargin;
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "0.00%";
                worksheet.Cell(row, 6).Style.Font.Bold = true;

                // Kolombreedtes aanpassen
                worksheet.Column(1).Width = 25;
                worksheet.Column(2).Width = 12;
                worksheet.Column(3).Width = 15;
                worksheet.Column(4).Width = 12;
                worksheet.Column(5).Width = 12;
                worksheet.Column(6).Width = 15;
                worksheet.Column(7).Width = 15;
                worksheet.Column(8).Width = 12;

                return SaveWorkbook(workbook, "Financieel_Rapport");
            }
        }

        // Genereert een Volledig Rapport met alle informatie en statistieken
        public string GenerateComprehensiveReport()
        {
            using (var workbook = new XLWorkbook())
            {
                // Blad 1: Overzicht
                var overviewSheet = workbook.Worksheets.Add("Overzicht");
                SetupWorksheet(overviewSheet);

                // Titel
                overviewSheet.Cell(1, 1).Value = "VOLLEDIG MAGAZIJNRAPPORT - OVERZICHT";
                overviewSheet.Range(1, 1, 1, 6).Merge();
                FormatTitle(overviewSheet.Cell(1, 1));

                // Datum
                overviewSheet.Cell(2, 1).Value = $"Gegenereerd op: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";
                overviewSheet.Cell(2, 1).Style.Font.Italic = true;

                // Statistieken
                int row = 4;
                var products = _context.Products.Where(p => !p.IsDeleted).ToList();
                var deliveries = _context.Deliveries.Where(d => !d.IsDeleted).ToList();
                var lowStockProducts = products.Where(p => p.StockQuantity < p.MinimumStock).Count();
                var activeAlerts = _context.StockAlerts.Where(sa => !sa.IsDeleted && sa.Status == StockAlertConstants.Status.Active).Count();

                overviewSheet.Cell(row, 1).Value = "STATISTIEKEN";
                overviewSheet.Cell(row, 1).Style.Font.Bold = true;
                overviewSheet.Cell(row, 1).Style.Font.Bold = true;

                row += 2;
                var stats = new[]
                {
                    ("Totaal Producten", products.Count.ToString()),
                    ("Producten met Lage Voorraad", lowStockProducts.ToString()),
                    ("Actieve Waarschuwingen", activeAlerts.ToString()),
                    ("Totaal Leveringen", deliveries.Count.ToString()),
                    ("Geplande Leveringen", deliveries.Count(d => d.Status == DeliveryConstants.Status.Planned).ToString()),
                    ("Verwerkte Leveringen", deliveries.Count(d => d.IsProcessed).ToString())
                };

                foreach (var (label, value) in stats)
                {
                    overviewSheet.Cell(row, 1).Value = label;
                    overviewSheet.Cell(row, 2).Value = value;
                    overviewSheet.Cell(row, 2).Style.Font.Bold = true;
                    row++;
                }

                // Financieel Overzicht
                row += 2;
                overviewSheet.Cell(row, 1).Value = "FINANCIEEL OVERZICHT";
                overviewSheet.Cell(row, 1).Style.Font.Bold = true;

                row += 2;
                decimal totalPurchaseValue = products.Sum(p => p.StockQuantity * p.PurchasePrice);
                decimal totalSellingValue = products.Sum(p => p.StockQuantity * p.SellingPrice);
                decimal potentialProfit = totalSellingValue - totalPurchaseValue;

                var financialStats = new[]
                {
                    ("Totale Inkoopwaarde Voorraad", totalPurchaseValue.ToString("€#,##0.00")),
                    ("Totale Verkoopwaarde Voorraad", totalSellingValue.ToString("€#,##0.00")),
                    ("Potentiële Winst", potentialProfit.ToString("€#,##0.00"))
                };

                foreach (var (label, value) in financialStats)
                {
                    overviewSheet.Cell(row, 1).Value = label;
                    overviewSheet.Cell(row, 2).Value = value;
                    overviewSheet.Cell(row, 2).Style.Font.Bold = true;
                    row++;
                }

                overviewSheet.Column(1).Width = 30;
                overviewSheet.Column(2).Width = 20;

                // Blad 2: Inventarisdetails
                var inventorySheet = workbook.Worksheets.Add("Inventaris");
                SetupWorksheet(inventorySheet);

                inventorySheet.Cell(1, 1).Value = "GEDETAILLEERDE INVENTARIS";
                inventorySheet.Range(1, 1, 1, 7).Merge();
                FormatTitle(inventorySheet.Cell(1, 1));

                row = 4;
                var invHeaders = new[] { "Product", "SKU", "Categorie", "Voorraad", "Min.", "Inkoopprijs", "Totale Waarde" };
                for (int col = 1; col <= invHeaders.Length; col++)
                {
                    var cell = inventorySheet.Cell(row, col);
                    cell.Value = invHeaders[col - 1];
                    FormatHeader(cell);
                }

                row = 5;
                foreach (var product in products.OrderBy(p => p.ProductName))
                {
                    decimal totalValue = product.StockQuantity * product.PurchasePrice;

                    inventorySheet.Cell(row, 1).Value = product.ProductName;
                    inventorySheet.Cell(row, 2).Value = product.SKU;
                    inventorySheet.Cell(row, 3).Value = product.Category;
                    inventorySheet.Cell(row, 4).Value = product.StockQuantity;
                    inventorySheet.Cell(row, 5).Value = product.MinimumStock;
                    inventorySheet.Cell(row, 6).Value = product.PurchasePrice;
                    inventorySheet.Cell(row, 6).Style.NumberFormat.Format = "€#,##0.00";
                    inventorySheet.Cell(row, 7).Value = totalValue;
                    inventorySheet.Cell(row, 7).Style.NumberFormat.Format = "€#,##0.00";

                    if (product.StockQuantity < product.MinimumStock)
                    {
                        inventorySheet.Row(row).Style.Fill.BackgroundColor = XLColor.Orange;
                    }

                    row++;
                }

                for (int col = 1; col <= invHeaders.Length; col++)
                {
                    inventorySheet.Column(col).Width = 15;
                }

                // Blad 3: Recente Leveringen
                var deliveriesSheet = workbook.Worksheets.Add("Leveringen");
                SetupWorksheet(deliveriesSheet);

                deliveriesSheet.Cell(1, 1).Value = "LEVERINGSOVERZICHT";
                deliveriesSheet.Range(1, 1, 1, 8).Merge();
                FormatTitle(deliveriesSheet.Cell(1, 1));

                row = 4;
                var delHeaders = new[] { "Referentie", "Type", "Partner", "Verwacht", "Werkelijk", "Status", "Bedrag", "Items" };
                for (int col = 1; col <= delHeaders.Length; col++)
                {
                    var cell = deliveriesSheet.Cell(row, col);
                    cell.Value = delHeaders[col - 1];
                    FormatHeader(cell);
                }

                row = 5;
                foreach (var delivery in deliveries.OrderByDescending(d => d.ExpectedDeliveryDate).Take(20))
                {
                    var itemCount = _context.DeliveryItems.Where(di => di.DeliveryId == delivery.DeliveryId && !di.IsDeleted).Count();

                    deliveriesSheet.Cell(row, 1).Value = delivery.ReferenceNumber;
                    deliveriesSheet.Cell(row, 2).Value = delivery.DeliveryType == DeliveryConstants.Types.Incoming ? "Inkomend" : "Uitgaand";
                    deliveriesSheet.Cell(row, 3).Value = delivery.DeliveryType == DeliveryConstants.Types.Incoming 
                        ? delivery.Supplier?.SupplierName ?? "N/A" 
                        : delivery.Customer?.CustomerName ?? "N/A";
                    deliveriesSheet.Cell(row, 4).Value = delivery.ExpectedDeliveryDate;
                    deliveriesSheet.Cell(row, 4).Style.NumberFormat.Format = "dd-MM-yyyy";
                    if (delivery.ActualDeliveryDate.HasValue)
                    {
                        deliveriesSheet.Cell(row, 5).Value = delivery.ActualDeliveryDate.Value;
                        deliveriesSheet.Cell(row, 5).Style.NumberFormat.Format = "dd-MM-yyyy";
                    }
                    deliveriesSheet.Cell(row, 6).Value = delivery.Status;
                    deliveriesSheet.Cell(row, 7).Value = delivery.TotalAmount;
                    deliveriesSheet.Cell(row, 7).Style.NumberFormat.Format = "€#,##0.00";
                    deliveriesSheet.Cell(row, 8).Value = itemCount;

                    if (delivery.Status == DeliveryConstants.Status.Delivered)
                    {
                        deliveriesSheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    }
                    else if (delivery.Status == DeliveryConstants.Status.Cancelled)
                    {
                        deliveriesSheet.Row(row).Style.Fill.BackgroundColor = XLColor.Red;
                    }

                    row++;
                }

                for (int col = 1; col <= delHeaders.Length; col++)
                {
                    deliveriesSheet.Column(col).Width = 14;
                }

                return SaveWorkbook(workbook, "Volledig_Magazijnrapport");
            }
        }

        // Hulpmethode om worksheet in te stellen met randen en uitlijning
        private void SetupWorksheet(IXLWorksheet worksheet)
        {
            worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
        }

        // Hulpmethode om titelcellen te formatteren
        private void FormatTitle(IXLCell cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        // Hulpmethode om koptekstcellen te formatteren
        private void FormatHeader(IXLCell cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Hulpmethode om het werkboek op te slaan en het bestandspad te retourneren
        private string SaveWorkbook(XLWorkbook workbook, string reportName)
        {
            string fileName = $"{reportName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            string filePath = Path.Combine(_reportsDirectory, fileName);
            workbook.SaveAs(filePath);
            return filePath;
        }
    }
}
