using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace SuntoryManagementSystem.Models
{
    public class SuntoryDbContext : DbContext
    {
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<DeliveryItem> DeliveryItems { get; set; }
        public DbSet<StockAdjustment> StockAdjustments { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //string connectionString = "Server=localhost;Database=SuntoryManagementDb;User Id=sa;Password=Your_password123;MultipleActiveResultSets=true";
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=SuntoryManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true";
            
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Voorkom multiple cascade paths door enkele relaties op Restrict te zetten
            modelBuilder.Entity<DeliveryItem>()
                .HasOne<Product>()
                .WithMany()
                .HasForeignKey(di => di.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Delivery>()
                .HasOne<Vehicle>()
                .WithMany()
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public static void Seeder(SuntoryDbContext context)
        {
            if (!context.Suppliers.Any())
            {
                context.Suppliers.AddRange(Supplier.SeedingData());
                context.SaveChanges();
            }

            if (!context.Vehicles.Any())
            {
                context.Vehicles.AddRange(Vehicle.SeedingData());
                context.SaveChanges();
            }

            if (!context.Products.Any())
            {
                context.Products.AddRange(Product.SeedingData());
                context.SaveChanges();
            }

            if (!context.Deliveries.Any())
            {
                context.Deliveries.AddRange(Delivery.SeedingData());
                context.SaveChanges();
            }

            if (!context.DeliveryItems.Any())
            {
                context.DeliveryItems.AddRange(DeliveryItem.SeedingData());
                context.SaveChanges();
            }

            if (!context.StockAdjustments.Any())
            {
                context.StockAdjustments.AddRange(StockAdjustment.SeedingData());
                context.SaveChanges();
            }

            if (!context.StockAlerts.Any())
            {
                context.StockAlerts.AddRange(StockAlert.SeedingData());
                context.SaveChanges();
            }
        }
    }
}