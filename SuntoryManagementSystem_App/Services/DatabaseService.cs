using Microsoft.EntityFrameworkCore;
using SuntoryManagementSystem_App.Data;
using SuntoryManagementSystem.Models;
using System.Diagnostics;

namespace SuntoryManagementSystem_App.Services;

public class DatabaseService
{
    private readonly LocalDbContext _context;
    
    public DatabaseService(LocalDbContext context)
    {
        _context = context;
    }
    
    public async Task InitializeAsync()
    {
        try
        {
            Debug.WriteLine("DatabaseService: Starting initialization...");
            
            // TEMPORARY FIX: Force delete and recreate database
            // This ensures a clean slate with correct schema
            // TODO: Remove this line after first successful run!
            await _context.Database.EnsureDeletedAsync();
            Debug.WriteLine("DatabaseService: Old database deleted");
            
            // Apply all pending migrations
            await _context.Database.MigrateAsync();
            Debug.WriteLine("DatabaseService: Migrations applied successfully");
            
            // Seed data ONLY if database is empty
            if (!await _context.Products.AnyAsync())
            {
                Debug.WriteLine("DatabaseService: Database is empty, seeding data...");
                SeedData();
                Debug.WriteLine("DatabaseService: Seeding completed");
            }
            else
            {
                Debug.WriteLine("DatabaseService: Database already contains data, skipping seed");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Database init error: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw; // Re-throw to see full error in output
        }
    }
    
    private void SeedData()
    {
        // HERGEBRUIK exact dezelfde seeding methods als SuntoryDbContext!
        
        if (!_context.Suppliers.Any())
        {
            Debug.WriteLine("Seeding Suppliers...");
            _context.Suppliers.AddRange(Supplier.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.Customers.Any())
        {
            Debug.WriteLine("Seeding Customers...");
            _context.Customers.AddRange(Customer.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.Vehicles.Any())
        {
            Debug.WriteLine("Seeding Vehicles...");
            _context.Vehicles.AddRange(Vehicle.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.Products.Any())
        {
            Debug.WriteLine("Seeding Products...");
            _context.Products.AddRange(Product.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.Deliveries.Any())
        {
            Debug.WriteLine("Seeding Deliveries...");
            _context.Deliveries.AddRange(Delivery.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.DeliveryItems.Any())
        {
            Debug.WriteLine("Seeding DeliveryItems...");
            _context.DeliveryItems.AddRange(DeliveryItem.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.StockAdjustments.Any())
        {
            Debug.WriteLine("Seeding StockAdjustments...");
            _context.StockAdjustments.AddRange(StockAdjustment.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.StockAlerts.Any())
        {
            Debug.WriteLine("Seeding StockAlerts...");
            _context.StockAlerts.AddRange(StockAlert.SeedingData());
            _context.SaveChanges();
        }
        
        Debug.WriteLine("All seeding operations completed successfully");
    }
}
