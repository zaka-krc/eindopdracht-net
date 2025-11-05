// ============================================================================
// SUNTORY MANAGEMENT SYSTEM (SMS)
// Program.cs - Console Application voor Database Testing
// ============================================================================

using SuntoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace SuntoryManagementSystem_Cons
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=".PadRight(70, '='));
            Console.WriteLine("SUNTORY MANAGEMENT SYSTEM - DATABASE TEST");
            Console.WriteLine("=".PadRight(70, '='));
            Console.WriteLine();

            using (var context = new SuntoryDbContext())
            {
                // Seed de database
                SuntoryDbContext.Seeder(context);

                // =================================================================
                // 1. TOON ALLE SUPPLIERS
                // =================================================================
                Console.WriteLine("\n" + "━".PadRight(70, '━'));
                Console.WriteLine("ALLE LEVERANCIERS:");
                Console.WriteLine("━".PadRight(70, '━'));
                
                var alleSuppliers = context.Suppliers.Where(s => !s.IsDeleted);
                foreach (var supplier in alleSuppliers)
                {
                    Console.WriteLine("- " + supplier);
                }

                // =================================================================
                // 2. FILTER ACTIEVE SUPPLIERS
                // =================================================================
                // Gebruik Where met een gedelegeerde functie
                var suppliers = context.Suppliers.Where(IsActive);
                    bool IsActive(Supplier s)
                    {
                        return s.Status == "Active" && !s.IsDeleted;
                    }

                // Doe exact hetzelfde met een anonieme delegate
                suppliers = context.Suppliers
                    .Where(delegate (Supplier s) { return s.Status == "Active" && !s.IsDeleted; });

                // Doe weer exact hetzelfde met een lambda-expressie
                suppliers = context.Suppliers
                    .Where(s => s.Status == "Active" && !s.IsDeleted)
                    .OrderBy(s => s.SupplierName);

                Console.WriteLine("\n" + "━".PadRight(70, '━'));
                Console.WriteLine("ALLEEN ACTIEVE LEVERANCIERS:");
                Console.WriteLine("━".PadRight(70, '━'));
                foreach (var supplier in suppliers)
                {
                    Console.WriteLine(supplier);
                }

                // =================================================================
                // 3. TOON ALLE PRODUCTEN MET LAGE VOORRAAD
                // =================================================================
                var productenMetLageVoorraad = context.Products
                    .Where(p => p.StockQuantity < p.MinimumStock && p.IsActive && !p.IsDeleted)
                    .OrderBy(p => p.StockQuantity);

                Console.WriteLine("\n" + "━".PadRight(70, '━'));
                Console.WriteLine("PRODUCTEN MET LAGE VOORRAAD:");
                Console.WriteLine("━".PadRight(70, '━'));
                foreach (var product in productenMetLageVoorraad)
                {
                    Console.WriteLine($"- {product} (Voorraad: {product.StockQuantity}/{product.MinimumStock})");
                }

                // =================================================================
                // 4. TOON ALLE LEVERINGEN DIE NOG NIET ZIJN VERWERKT
                // =================================================================
                var onverwerkteLeveringen = context.Deliveries
                    .Where(d => !d.IsProcessed && !d.IsDeleted)
                    .OrderBy(d => d.ExpectedDeliveryDate);

                Console.WriteLine("\n" + "━".PadRight(70, '━'));
                Console.WriteLine("ONVERWERKTE LEVERINGEN:");
                Console.WriteLine("━".PadRight(70, '━'));
                foreach (var delivery in onverwerkteLeveringen)
                {
                    Console.WriteLine(delivery);
                }

                // =================================================================
                // 5. TOEVOEGEN VAN EEN NIEUW PRODUCT
                // =================================================================
                Product nieuwProduct = new Product()
                {
                    ProductName = "Pepsi Cola 330ml",
                    Description = "Klassieke cola frisdrank",
                    SKU = "PEP-330-001",
                    Category = "Frisdrank",
                    PurchasePrice = 0.40m,
                    SellingPrice = 0.90m,
                    StockQuantity = 150,
                    MinimumStock = 50,
                    SupplierId = 1,
                    IsActive = true
                };
                
                context.Add(nieuwProduct);  //zelfde als context.Products.Add(nieuwProduct);
                context.SaveChanges();
                
                Console.WriteLine("\n" + "━".PadRight(70, '━'));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ EEN NIEUW PRODUCT WERD TOEGEVOEGD:");
                Console.ResetColor();
                Console.WriteLine("━".PadRight(70, '━'));
                Console.WriteLine(nieuwProduct);

                // =================================================================
                // 6. WIJZIGEN VAN EEN PRODUCT
                // =================================================================
                Product? teWijzigen = context.Products.FirstOrDefault(p => p.SKU == "PEP-330-001");
                if (teWijzigen != null)
                {
                    teWijzigen.StockQuantity = 200;
                    teWijzigen.SellingPrice = 0.95m;
                    teWijzigen.Description = "Verfrissende Pepsi Cola - nieuwe voorraad";
                    
                    context.Update(teWijzigen);  //zelfde als context.Products.Update(teWijzigen);
                    context.SaveChanges();

                    Console.WriteLine("\n" + "━".PadRight(70, '━'));
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("✓ PRODUCT WERD GEWIJZIGD:");
                    Console.ResetColor();
                    Console.WriteLine("━".PadRight(70, '━'));
                    Console.WriteLine(teWijzigen);
                }

                // =================================================================
                // 7. TOON ALLE PRODUCTEN VAN LEVERANCIER "Suntory"
                // =================================================================
                var suntorySupplier = context.Suppliers.FirstOrDefault(s => s.SupplierName.Contains("Suntory"));
                if (suntorySupplier != null)
                {
                    var suntoryProducten = context.Products
                        .Where(p => p.SupplierId == suntorySupplier.SupplierId)
                        .OrderBy(p => p.ProductName);

                    Console.WriteLine("\n" + "━".PadRight(70, '━'));
                    Console.WriteLine($"PRODUCTEN VAN {suntorySupplier.SupplierName}:");
                    Console.WriteLine("━".PadRight(70, '━'));
                    foreach (var product in suntoryProducten)
                    {
                        Console.Write(product);
                        Console.WriteLine($"  (Leverancier: {suntorySupplier.SupplierName})");
                    }
                }

                // =================================================================
                // 8. TOEVOEGEN VAN EEN NIEUWE STOCK ADJUSTMENT
                // =================================================================
                StockAdjustment nieuweAanpassing = new StockAdjustment()
                {
                    ProductId = nieuwProduct.ProductId,
                    AdjustmentType = "Addition",
                    QuantityChange = 50,
                    PreviousQuantity = nieuwProduct.StockQuantity,
                    NewQuantity = nieuwProduct.StockQuantity + 50,
                    Reason = "Nieuwe voorraad aangevuld via console test",
                    AdjustedBy = "System Admin"
                };

                context.Add(nieuweAanpassing);
                context.SaveChanges();

                Console.WriteLine("\n" + "━".PadRight(70, '━'));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ VOORRAAD AANPASSING TOEGEVOEGD:");
                Console.ResetColor();
                Console.WriteLine("━".PadRight(70, '━'));
                Console.WriteLine(nieuweAanpassing);

                // =================================================================
                // 9. TOON ALLE BESCHIKBARE VOERTUIGEN
                // =================================================================
                var beschikbareVoertuigen = context.Vehicles
                    .Where(v => v.IsAvailable && !v.IsDeleted)
                    .OrderBy(v => v.Capacity);

                Console.WriteLine("\n" + "━".PadRight(70, '━'));
                Console.WriteLine("BESCHIKBARE VOERTUIGEN:");
                Console.WriteLine("━".PadRight(70, '━'));
                foreach (var vehicle in beschikbareVoertuigen)
                {
                    Console.WriteLine($"- {vehicle} - Capaciteit: {vehicle.Capacity} kg");
                }

                // =================================================================
                // 10. TOON ACTIEVE STOCK ALERTS
                // =================================================================
                var actiefAlerts = context.StockAlerts
                    .Include(sa => sa.Product)
                    .Where(sa => sa.Status == "Active" && !sa.IsDeleted)
                    .OrderBy(sa => sa.Product.StockQuantity);

                Console.WriteLine("\n" + "━".PadRight(70, '━'));
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠ ACTIEVE VOORRAAD WAARSCHUWINGEN:");
                Console.ResetColor();
                Console.WriteLine("━".PadRight(70, '━'));
                foreach (var alert in actiefAlerts)
                {
                    Console.WriteLine($"{alert} - Product: {alert.Product?.ProductName} (Voorraad: {alert.Product?.StockQuantity}/{alert.Product?.MinimumStock})");
                }

                // =================================================================
                // EINDRESULTAAT
                // =================================================================
                Console.WriteLine("\n" + "=".PadRight(70, '='));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ ALLE DATABASE OPERATIES SUCCESVOL UITGEVOERD!");
                Console.ResetColor();
                Console.WriteLine("=".PadRight(70, '='));
                Console.WriteLine($"\nTotaal aantal suppliers: {context.Suppliers.Count()}");
                Console.WriteLine($"Totaal aantal producten: {context.Products.Count()}");
                Console.WriteLine($"Totaal aantal voertuigen: {context.Vehicles.Count()}");
                Console.WriteLine($"Totaal aantal leveringen: {context.Deliveries.Count()}");
                Console.WriteLine($"Totaal aantal stock adjustments: {context.StockAdjustments.Count()}");
                Console.WriteLine($"Totaal aantal stock alerts: {context.StockAlerts.Count()}");
            }

            Console.WriteLine("\nDruk op een toets om af te sluiten...");
            Console.ReadKey();
        }
    }
}