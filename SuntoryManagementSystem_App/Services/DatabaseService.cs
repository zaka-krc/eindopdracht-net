using Microsoft.EntityFrameworkCore;
using SuntoryManagementSystem_App.Data;
using SuntoryManagementSystem.Models;

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
            // Zorg dat database bestaat
            await _context.Database.EnsureCreatedAsync();
            
            // Seed data ALLEEN als database leeg is
            if (!_context.Products.Any())
            {
                SeedData();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database init error: {ex.Message}");
        }
    }
    
    private void SeedData()
    {
        // HERGEBRUIK exact dezelfde seeding methods als SuntoryDbContext!
        
        if (!_context.Suppliers.Any())
        {
            _context.Suppliers.AddRange(Supplier.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.Customers.Any())
        {
            _context.Customers.AddRange(Customer.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.Vehicles.Any())
        {
            _context.Vehicles.AddRange(Vehicle.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.Products.Any())
        {
            _context.Products.AddRange(Product.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.Deliveries.Any())
        {
            _context.Deliveries.AddRange(Delivery.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.DeliveryItems.Any())
        {
            _context.DeliveryItems.AddRange(DeliveryItem.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.StockAdjustments.Any())
        {
            _context.StockAdjustments.AddRange(StockAdjustment.SeedingData());
            _context.SaveChanges();
        }

        if (!_context.StockAlerts.Any())
        {
            _context.StockAlerts.AddRange(StockAlert.SeedingData());
            _context.SaveChanges();
        }
    }
}
