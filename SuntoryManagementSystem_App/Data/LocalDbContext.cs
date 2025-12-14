using Microsoft.EntityFrameworkCore;
using SuntoryManagementSystem.Models;

namespace SuntoryManagementSystem_App.Data;

public class LocalDbContext : DbContext
{
    // Exacte kopie van DbSets uit SuntoryDbContext (zonder Identity)
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }
    public DbSet<DeliveryItem> DeliveryItems { get; set; }
    public DbSet<StockAdjustment> StockAdjustments { get; set; }
    public DbSet<StockAlert> StockAlerts { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // SQLite voor MAUI app (offline storage)
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbPath = Path.Join(path, "SuntoryApp.db");
        options.UseSqlite($"Data Source={dbPath}");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // KOPIEER EXACT de relationships uit SuntoryDbContext
        // (SQLite ondersteunt niet alle SQL Server features, maar deze zijn compatible)
        
        // ===================================================================
        // DELIVERY RELATIONSHIPS
        // ===================================================================
        
        // Delivery -> Supplier (optional, voor Incoming deliveries)
        modelBuilder.Entity<Delivery>()
            .HasOne(d => d.Supplier)
            .WithMany()
            .HasForeignKey(d => d.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // Delivery -> Customer (optional, voor Outgoing deliveries)
        modelBuilder.Entity<Delivery>()
            .HasOne(d => d.Customer)
            .WithMany()
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Delivery -> Vehicle (optional)
        modelBuilder.Entity<Delivery>()
            .HasOne(d => d.Vehicle)
            .WithMany()
            .HasForeignKey(d => d.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===================================================================
        // DELIVERY ITEM RELATIONSHIPS
        // ===================================================================
        
        // DeliveryItem -> Delivery (required)
        modelBuilder.Entity<DeliveryItem>()
            .HasOne(di => di.Delivery)
            .WithMany(d => d.DeliveryItems)
            .HasForeignKey(di => di.DeliveryId)
            .OnDelete(DeleteBehavior.Cascade);

        // DeliveryItem -> Product (required)
        modelBuilder.Entity<DeliveryItem>()
            .HasOne(di => di.Product)
            .WithMany()
            .HasForeignKey(di => di.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===================================================================
        // PRODUCT RELATIONSHIPS
        // ===================================================================
        
        // Product -> Supplier (required)
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===================================================================
        // STOCK ADJUSTMENT RELATIONSHIPS
        // ===================================================================
        
        // StockAdjustment -> Product (required)
        modelBuilder.Entity<StockAdjustment>()
            .HasOne(sa => sa.Product)
            .WithMany()
            .HasForeignKey(sa => sa.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===================================================================
        // STOCK ALERT RELATIONSHIPS
        // ===================================================================
        
        // StockAlert -> Product (required)
        modelBuilder.Entity<StockAlert>()
            .HasOne(sa => sa.Product)
            .WithMany()
            .HasForeignKey(sa => sa.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
