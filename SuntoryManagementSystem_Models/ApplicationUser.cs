using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SuntoryManagementSystem.Models
{
    // Gebruiker van het Suntory Management System
    // Afgeleid van IdentityUser met extra bedrijfsspecifieke eigenschappen
    public class ApplicationUser : IdentityUser
    {
        // Volledige naam van de gebruiker
        [Required(ErrorMessage = "Volledige naam is verplicht")]
        [StringLength(100, ErrorMessage = "Naam mag maximaal 100 tekens zijn")]
        [Display(Name = "Volledige Naam")]
        public string FullName { get; set; } = string.Empty;

        // Afdeling waar de gebruiker werkt
        [StringLength(50)]
        [Display(Name = "Afdeling")]
        public string? Department { get; set; }

        // Functietitel van de gebruiker
        [StringLength(50)]
        [Display(Name = "Functie")]
        public string? JobTitle { get; set; }

        // Datum waarop de gebruiker is aangemaakt
        [Required]
        [Display(Name = "Aangemaakt op")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Laatste login datum
        [Display(Name = "Laatste Login")]
        [DataType(DataType.DateTime)]
        public DateTime? LastLoginDate { get; set; }

        // Is het account actief?
        [Required]
        [Display(Name = "Actief")]
        public bool IsActive { get; set; } = true;

        public override string ToString()
        {
            return $"{FullName} ({Email}) - {Department ?? "Geen afdeling"}";
        }

        // Seeding data voor test gebruikers
        // Wachtwoorden: Admin@123, Manager@123, Employee@123
        // Minimaal 1 gebruiker per rol (Administrator, Manager, Employee)
        public static List<ApplicationUser> SeedingData()
        {
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var list = new List<ApplicationUser>();

            // ============================================================
            // ADMINISTRATOR ROL (1 gebruiker)
            // ============================================================
            
            var admin = new ApplicationUser
            {
                Id = "admin-001",
                UserName = "admin@suntory.com",
                NormalizedUserName = "ADMIN@SUNTORY.COM",
                Email = "admin@suntory.com",
                NormalizedEmail = "ADMIN@SUNTORY.COM",
                EmailConfirmed = true,
                FullName = "Admin",  // Match met StockAdjustment.AdjustedBy
                Department = "IT",
                JobTitle = "System Administrator",
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-6),
                LastLoginDate = DateTime.Now.AddHours(-2),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin@123");
            list.Add(admin);

            // ============================================================
            // MANAGER ROL (2 gebruikers)
            // ============================================================
            
            var manager1 = new ApplicationUser
            {
                Id = "manager-001",
                UserName = "manager@suntory.com",
                NormalizedUserName = "MANAGER@SUNTORY.COM",
                Email = "manager@suntory.com",
                NormalizedEmail = "MANAGER@SUNTORY.COM",
                EmailConfirmed = true,
                FullName = "Warehouse Manager",  // Match met StockAdjustment.AdjustedBy
                Department = "Logistics",
                JobTitle = "Warehouse Manager",
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-5),
                LastLoginDate = DateTime.Now.AddDays(-1),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            manager1.PasswordHash = passwordHasher.HashPassword(manager1, "Manager@123");
            list.Add(manager1);

            var manager2 = new ApplicationUser
            {
                Id = "manager-002",
                UserName = "sales.manager@suntory.com",
                NormalizedUserName = "SALES.MANAGER@SUNTORY.COM",
                Email = "sales.manager@suntory.com",
                NormalizedEmail = "SALES.MANAGER@SUNTORY.COM",
                EmailConfirmed = true,
                FullName = "Lisa Bakker",
                Department = "Sales",
                JobTitle = "Sales Manager",
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-4),
                LastLoginDate = DateTime.Now.AddDays(-2),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            manager2.PasswordHash = passwordHasher.HashPassword(manager2, "Manager@123");
            list.Add(manager2);

            // ============================================================
            // EMPLOYEE ROL (5 gebruikers)
            // ============================================================
            
            var employee1 = new ApplicationUser
            {
                Id = "employee-001",
                UserName = "employee@suntory.com",
                NormalizedUserName = "EMPLOYEE@SUNTORY.COM",
                Email = "employee@suntory.com",
                NormalizedEmail = "EMPLOYEE@SUNTORY.COM",
                EmailConfirmed = true,
                FullName = "Peter de Vries",
                Department = "Warehouse",
                JobTitle = "Warehouse Employee",
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-3),
                LastLoginDate = DateTime.Now.AddHours(-5),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            employee1.PasswordHash = passwordHasher.HashPassword(employee1, "Employee@123");
            list.Add(employee1);

            var employee2 = new ApplicationUser
            {
                Id = "employee-002",
                UserName = "thomas.wit@suntory.com",
                NormalizedUserName = "THOMAS.WIT@SUNTORY.COM",
                Email = "thomas.wit@suntory.com",
                NormalizedEmail = "THOMAS.WIT@SUNTORY.COM",
                EmailConfirmed = true,
                FullName = "Thomas de Wit",
                Department = "Logistics",
                JobTitle = "Logistics Coordinator",
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-2),
                LastLoginDate = DateTime.Now.AddDays(-3),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            employee2.PasswordHash = passwordHasher.HashPassword(employee2, "Employee@123");
            list.Add(employee2);

            var employee3 = new ApplicationUser
            {
                Id = "employee-003",
                UserName = "emma.visser@suntory.com",
                NormalizedUserName = "EMMA.VISSER@SUNTORY.COM",
                Email = "emma.visser@suntory.com",
                NormalizedEmail = "EMMA.VISSER@SUNTORY.COM",
                EmailConfirmed = true,
                FullName = "Emma Visser",
                Department = "Warehouse",
                JobTitle = "Stock Controller",
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-1),
                LastLoginDate = DateTime.Now.AddHours(-8),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            employee3.PasswordHash = passwordHasher.HashPassword(employee3, "Employee@123");
            list.Add(employee3);

            var employee4 = new ApplicationUser
            {
                Id = "employee-004",
                UserName = "michael.jong@suntory.com",
                NormalizedUserName = "MICHAEL.JONG@SUNTORY.COM",
                Email = "michael.jong@suntory.com",
                NormalizedEmail = "MICHAEL.JONG@SUNTORY.COM",
                EmailConfirmed = true,
                FullName = "Michael de Jong",
                Department = "Warehouse",
                JobTitle = "Former Employee",
                IsActive = false,  // Inactieve gebruiker voor testing
                CreatedDate = DateTime.Now.AddMonths(-8),
                LastLoginDate = DateTime.Now.AddMonths(-2),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            employee4.PasswordHash = passwordHasher.HashPassword(employee4, "Employee@123");
            list.Add(employee4);

            var employee5 = new ApplicationUser
            {
                Id = "employee-005",
                UserName = "anna.smit@suntory.com",
                NormalizedUserName = "ANNA.SMIT@SUNTORY.COM",
                Email = "anna.smit@suntory.com",
                NormalizedEmail = "ANNA.SMIT@SUNTORY.COM",
                EmailConfirmed = true,
                FullName = "Anna Smit",
                Department = "Finance",
                JobTitle = "Financial Controller",
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-4),
                LastLoginDate = DateTime.Now.AddDays(-1),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            employee5.PasswordHash = passwordHasher.HashPassword(employee5, "Employee@123");
            list.Add(employee5);

            return list;
        }
    }
}
