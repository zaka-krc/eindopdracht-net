using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Linq;
using SuntoryManagementSystem.Models;

namespace SuntoryManagementSystem_Models.Data
{
    public class SuntoryDbContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor voor ASP.NET Core dependency injection
        public SuntoryDbContext(DbContextOptions<SuntoryDbContext> options) 
            : base(options)
        {
        }
        
        // Parameterless constructor voor WPF en MAUI
        public SuntoryDbContext() : base()
        {
        }

        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<DeliveryItem> DeliveryItems { get; set; }
        public DbSet<StockAdjustment> StockAdjustments { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Alleen configureren als nog niet geconfigureerd (voor WPF en MAUI gebruik)
            if (!optionsBuilder.IsConfigured)
            {
                //string connectionString = "Server=localhost;Database=SuntoryManagementDb;User Id=sa;Password=Your_password123;MultipleActiveResultSets=true";
                string connectionString = "Server=tcp:zakariaserverdbsun.database.windows.net,1433;Initial Catalog=free-sql-db-7596343;Persist Security Info=False;User ID=admin123;Password=Zakaria2003;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
                
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===================================================================
            // DELIVERY RELATIONSHIPS
            // ===================================================================
            
            // Delivery -> Supplier (optional, voor Incoming deliveries)
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Supplier)
                .WithMany(s => s.Deliveries)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Delivery -> Customer (optional, voor Outgoing deliveries)
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Customer)
                .WithMany(c => c.Deliveries)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Delivery -> Vehicle (optional)
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Vehicle)
                .WithMany(v => v.Deliveries)
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
                .WithMany(s => s.Products)
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

        public static void Seeder(SuntoryDbContext context)
        {
            // Seed Identity Roles
            SeedIdentityRoles(context);
            
            // Seed Identity Users
            SeedIdentityUsers(context);

            if (!context.Suppliers.Any())
            {
                context.Suppliers.AddRange(Supplier.SeedingData());
                context.SaveChanges();
            }

            if (!context.Customers.Any())
            {
                context.Customers.AddRange(Customer.SeedingData());
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

        private static void SeedIdentityRoles(SuntoryDbContext context)
        {
            // Check of rollen al bestaan
            if (context.Roles.Any())
                return;

            // Maak de standaard rollen aan MET VASTE ID's
            var roles = new[]
            {
                new IdentityRole
                {
                    Id = "role-admin-001",
                    Name = "Administrator",
                    NormalizedName = "ADMINISTRATOR",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new IdentityRole
                {
                    Id = "role-manager-001",
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new IdentityRole
                {
                    Id = "role-employee-001",
                    Name = "Employee",
                    NormalizedName = "EMPLOYEE",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            };

            context.Roles.AddRange(roles);
            context.SaveChanges();
        }

        private static void SeedIdentityUsers(SuntoryDbContext context)
        {
            // Check of gebruikers al bestaan
            if (context.Users.Any())
                return;

            // Gebruik de SeedingData methode van ApplicationUser
            var users = ApplicationUser.SeedingData();
            context.Users.AddRange(users);
            context.SaveChanges();

            // Gebruik VASTE rol ID's (consistent met SeedIdentityRoles)
            const string adminRoleId = "role-admin-001";
            const string managerRoleId = "role-manager-001";
            const string employeeRoleId = "role-employee-001";

            // Wijs rollen toe aan gebruikers op basis van hun ID
            // Minimaal 1 gebruiker per rol zoals gevraagd
            var userRoles = new List<IdentityUserRole<string>>
            {
                // Administrator (1 gebruiker)
                new IdentityUserRole<string> { UserId = "admin-001", RoleId = adminRoleId },

                // Manager (2 gebruikers)
                new IdentityUserRole<string> { UserId = "manager-001", RoleId = managerRoleId },
                new IdentityUserRole<string> { UserId = "manager-002", RoleId = managerRoleId },

                // Employee (5 gebruikers)
                new IdentityUserRole<string> { UserId = "employee-001", RoleId = employeeRoleId },
                new IdentityUserRole<string> { UserId = "employee-002", RoleId = employeeRoleId },
                new IdentityUserRole<string> { UserId = "employee-003", RoleId = employeeRoleId },
                new IdentityUserRole<string> { UserId = "employee-004", RoleId = employeeRoleId },
                new IdentityUserRole<string> { UserId = "employee-005", RoleId = employeeRoleId }
            };

            context.UserRoles.AddRange(userRoles);
            context.SaveChanges();
        }
    }
}