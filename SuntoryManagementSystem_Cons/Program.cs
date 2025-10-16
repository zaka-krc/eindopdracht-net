// ============================================================================
// SUNTORY MANAGEMENT SYSTEM (SMS)
// Program.cs - Test Console Application voor Seeding Data
// ============================================================================

using SuntoryManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace SuntoryManagementSystem_Cons
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=".PadRight(70, '='));
            Console.WriteLine("SUNTORY MANAGEMENT SYSTEM - SEEDER TEST");
            Console.WriteLine("=".PadRight(70, '='));
            Console.WriteLine();

            try
            {
                // Test Supplier Seeder
                TestSupplierSeeder();

                // Test Product Seeder
                TestProductSeeder();

                // Test Vehicle Seeder
                TestVehicleSeeder();

                // Test Delivery Seeder
                TestDeliverySeeder();

                // Test StockAlert Seeder
                TestStockAlertSeeder();

                // Test StockAdjustment Seeder
                TestStockAdjustmentSeeder();

                Console.WriteLine();
                Console.WriteLine("=".PadRight(70, '='));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ ALLE SEEDERS SUCCESVOL GETEST!");
                Console.ResetColor();
                Console.WriteLine("=".PadRight(70, '='));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n✗ FOUT: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.ResetColor();
            }

            Console.WriteLine("\nDruk op een toets om af te sluiten...");
            Console.ReadKey();
        }

        static void TestSupplierSeeder()
        {
            Console.WriteLine("━".PadRight(70, '━'));
            Console.WriteLine("TEST: Supplier Seeder");
            Console.WriteLine("━".PadRight(70, '━'));

            var suppliers = Supplier.SeedingData();

            Console.WriteLine($"Aantal suppliers aangemaakt: {suppliers.Count}");
            Console.WriteLine();

            foreach (var supplier in suppliers)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  • {supplier.SupplierName}");
                Console.ResetColor();
                Console.WriteLine($"    - Adres: {supplier.Address}, {supplier.PostalCode} {supplier.City}");
                Console.WriteLine($"    - Contact: {supplier.ContactPerson} ({supplier.Email})");
                Console.WriteLine($"    - Telefoon: {supplier.PhoneNumber}");
                Console.WriteLine($"    - Status: {supplier.Status}");
                Console.WriteLine($"    - ToString(): {supplier}");
                Console.WriteLine();
            }
        }

        static void TestProductSeeder()
        {
            Console.WriteLine("━".PadRight(70, '━'));
            Console.WriteLine("TEST: Product Seeder");
            Console.WriteLine("━".PadRight(70, '━'));

            var products = Product.SeedingData();

            Console.WriteLine($"Aantal producten aangemaakt: {products.Count}");
            Console.WriteLine();

            foreach (var product in products)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  • {product.ProductName}");
                Console.ResetColor();
                Console.WriteLine($"    - SKU: {product.SKU}");
                Console.WriteLine($"    - Categorie: {product.Category}");
                Console.WriteLine($"    - Beschrijving: {product.Description}");
                Console.WriteLine($"    - Inkoopprijs: €{product.PurchasePrice:F2} | Verkoopprijs: €{product.SellingPrice:F2}");
                Console.WriteLine($"    - Voorraad: {product.StockQuantity} (Min: {product.MinimumStock})");
                Console.WriteLine($"    - Leverancier ID: {product.SupplierId}");
                Console.WriteLine($"    - ToString(): {product}");
                Console.WriteLine();
            }
        }

        static void TestVehicleSeeder()
        {
            Console.WriteLine("━".PadRight(70, '━'));
            Console.WriteLine("TEST: Vehicle Seeder");
            Console.WriteLine("━".PadRight(70, '━'));

            var vehicles = Vehicle.SeedingData();

            Console.WriteLine($"Aantal voertuigen aangemaakt: {vehicles.Count}");
            Console.WriteLine();

            foreach (var vehicle in vehicles)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"  • {vehicle.Brand} {vehicle.Model}");
                Console.ResetColor();
                Console.WriteLine($"    - Kenteken: {vehicle.LicensePlate}");
                Console.WriteLine($"    - Type: {vehicle.VehicleType}");
                Console.WriteLine($"    - Capaciteit: {vehicle.Capacity} kg");
                Console.WriteLine($"    - Beschikbaar: {(vehicle.IsAvailable ? "Ja" : "Nee")}");
                Console.WriteLine($"    - Laatste onderhoud: {vehicle.LastMaintenanceDate?.ToString("dd-MM-yyyy") ?? "N/A"}");
                if (!string.IsNullOrEmpty(vehicle.Notes))
                    Console.WriteLine($"    - Opmerkingen: {vehicle.Notes}");
                Console.WriteLine($"    - ToString(): {vehicle}");
                Console.WriteLine();
            }
        }

        static void TestDeliverySeeder()
        {
            Console.WriteLine("━".PadRight(70, '━'));
            Console.WriteLine("TEST: Delivery Seeder");
            Console.WriteLine("━".PadRight(70, '━'));

            var deliveries = Delivery.SeedingData();

            Console.WriteLine($"Aantal leveringen aangemaakt: {deliveries.Count}");
            Console.WriteLine();

            foreach (var delivery in deliveries)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  • {delivery.ReferenceNumber}");
                Console.ResetColor();
                Console.WriteLine($"    - Leverancier ID: {delivery.SupplierId}");
                Console.WriteLine($"    - Status: {delivery.Status}");
                Console.WriteLine($"    - Verwachte levering: {delivery.ExpectedDeliveryDate:dd-MM-yyyy HH:mm}");
                if (delivery.ActualDeliveryDate.HasValue)
                    Console.WriteLine($"    - Werkelijke levering: {delivery.ActualDeliveryDate:dd-MM-yyyy HH:mm}");
                Console.WriteLine($"    - Totaalbedrag: €{delivery.TotalAmount:F2}");
                Console.WriteLine($"    - Verwerkt: {(delivery.IsProcessed ? "Ja" : "Nee")}");
                if (!string.IsNullOrEmpty(delivery.Notes))
                    Console.WriteLine($"    - Opmerkingen: {delivery.Notes}");
                Console.WriteLine($"    - ToString(): {delivery}");
                Console.WriteLine();
            }
        }

        static void TestStockAlertSeeder()
        {
            Console.WriteLine("━".PadRight(70, '━'));
            Console.WriteLine("TEST: StockAlert Seeder");
            Console.WriteLine("━".PadRight(70, '━'));

            var stockAlerts = StockAlert.SeedingData();

            Console.WriteLine($"Aantal voorraad waarschuwingen aangemaakt: {stockAlerts.Count}");
            Console.WriteLine();

            foreach (var alert in stockAlerts)
            {
                Console.ForegroundColor = alert.Status == "Active" ? ConsoleColor.Red : ConsoleColor.DarkGray;
                Console.WriteLine($"  • Product ID: {alert.ProductId}");
                Console.ResetColor();
                Console.WriteLine($"    - Huidige voorraad: {alert.CurrentStock}");
                Console.WriteLine($"    - Minimum voorraad: {alert.MinimumStock}");
                Console.WriteLine($"    - Status: {alert.Status}");
                Console.WriteLine($"    - Aangemaakt: {alert.CreatedDate:dd-MM-yyyy HH:mm}");
                if (alert.ResolvedDate.HasValue)
                    Console.WriteLine($"    - Opgelost: {alert.ResolvedDate:dd-MM-yyyy HH:mm}");
                if (!string.IsNullOrEmpty(alert.Notes))
                    Console.WriteLine($"    - Opmerkingen: {alert.Notes}");
                Console.WriteLine($"    - ToString(): {alert}");
                Console.WriteLine();
            }
        }

        static void TestStockAdjustmentSeeder()
        {
            Console.WriteLine("━".PadRight(70, '━'));
            Console.WriteLine("TEST: StockAdjustment Seeder");
            Console.WriteLine("━".PadRight(70, '━'));

            var stockAdjustments = StockAdjustment.SeedingData();

            Console.WriteLine($"Aantal voorraad aanpassingen aangemaakt: {stockAdjustments.Count}");
            Console.WriteLine();

            foreach (var adjustment in stockAdjustments)
            {
                Console.ForegroundColor = adjustment.QuantityChange > 0 ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"  • Product ID: {adjustment.ProductId} - {adjustment.AdjustmentType}");
                Console.ResetColor();
                Console.WriteLine($"    - Wijziging: {adjustment.QuantityChange:+#;-#;0} stuks");
                Console.WriteLine($"    - Voorraad: {adjustment.PreviousQuantity} → {adjustment.NewQuantity}");
                Console.WriteLine($"    - Datum: {adjustment.AdjustmentDate:dd-MM-yyyy HH:mm}");
                Console.WriteLine($"    - Reden: {adjustment.Reason}");
                Console.WriteLine($"    - Aangepast door: {adjustment.AdjustedBy}");
                Console.WriteLine($"    - ToString(): {adjustment}");
                Console.WriteLine();
            }
        }
    }
}