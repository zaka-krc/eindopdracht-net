using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuntoryManagementSystem.Models
{
    // Product - Producten die worden beheerd in het systeem
    public class Product
    {
        // Unieke identifier voor het product
        [Key]
        public int ProductId { get; set; }

        // Foreign Key naar Supplier
        [Required(ErrorMessage = "Leverancier is verplicht")]
        [Display(Name = "Leverancier")]
        public int SupplierId { get; set; }

        // De leverancier van dit product
        public Supplier? Supplier { get; set; }

        // Naam van het product
        [Required(ErrorMessage = "Productnaam is verplicht")]
        [StringLength(100, ErrorMessage = "Productnaam mag maximaal 100 tekens zijn")]
        [Display(Name = "Productnaam")]
        public string ProductName { get; set; } = string.Empty;

        // Beschrijving van het product
        [StringLength(500)]
        [Display(Name = "Beschrijving")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        // SKU (Stock Keeping Unit) - unieke productcode
        [Required(ErrorMessage = "SKU is verplicht")]
        [StringLength(50)]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty;

        // Categorie van het product (bijv. "Frisdrank", "Water", "Energiedrank")
        [Required]
        [StringLength(50)]
        [Display(Name = "Categorie")]
        public string Category { get; set; } = "Frisdrank";

        // Inkoopprijs per eenheid
        [Required(ErrorMessage = "Inkoopprijs is verplicht")]
        [Column(TypeName = "DECIMAL(10, 2)")]
        [Display(Name = "Inkoopprijs")]
        public decimal PurchasePrice { get; set; } = 0.00m;

        // Verkoopprijs per eenheid
        [Required(ErrorMessage = "Verkoopprijs is verplicht")]
        [Column(TypeName = "DECIMAL(10, 2)")]
        [Display(Name = "Verkoopprijs")]
        public decimal SellingPrice { get; set; } = 0.00m;

        // Huidige voorraad
        [Required]
        [Display(Name = "Voorraad")]
        public int StockQuantity { get; set; } = 0;

        // Minimale voorraad voordat een waarschuwing wordt gegeven
        [Required]
        [Display(Name = "Min. voorraad")]
        public int MinimumStock { get; set; } = 10;

        // Is het product actief?
        [Required]
        [Display(Name = "Actief")]
        public bool IsActive { get; set; } = true;

        // Datum en tijd van aanmaak
        [Required]
        [Display(Name = "Aangemaakt")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // SOFT DELETE PROPERTIES
        
        // Is dit product soft-deleted?
        [Required]
        [Display(Name = "Verwijderd")]
        public bool IsDeleted { get; set; } = false;

        // Datum en tijd van soft delete
        [Display(Name = "Verwijderd op")]
        [DataType(DataType.DateTime)]
        public DateTime? DeletedDate { get; set; }

        public override string ToString()
        {
            return $"{ProductId} - {ProductName} (SKU: {SKU}, Voorraad: {StockQuantity})";
        }

        public static List<Product> SeedingData()
        {
            var list = new List<Product>();
            list.AddRange(new[]
            {
                // Product 1: Orangina
                // Start: 200 
                // + INC-001: +100 = 300
                // + INC-002: +200 = 500
                // - OUT-001: -50 = 450
                // - OUT-002: -100 = 350
                // + levering 7: +5 = 355
                // Huidige voorraad: 355
                new Product 
                { 
                    ProductName = "Orangina Original 330ml",
                    Description = "Verfrissende sinaasappeldrank met pulp",
                    SKU = "ORG-330-001",
                    Category = "Frisdrank",
                    PurchasePrice = 0.45m,
                    SellingPrice = 0.95m,
                    StockQuantity = 355,
                    MinimumStock = 50,
                    SupplierId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-2)
                },
                // Product 2: Lucozade
                // Start: 195
                // + INC-001: +50 = 245
                // + INC-003: +80 = 325
                // - Damage: -15 = 310
                // - OUT-002: -80 = 230
                // + levering 8: +100 = 330
                // Huidige voorraad: 330
                new Product 
                { 
                    ProductName = "Lucozade Energy Original 380ml",
                    Description = "Energiedrank met glucose",
                    SKU = "LUC-380-001",
                    Category = "Energiedrank",
                    PurchasePrice = 0.65m,
                    SellingPrice = 1.25m,
                    StockQuantity = 330,
                    MinimumStock = 120,
                    SupplierId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-2)
                },
                // Product 3: Ribena
                // Start: 120
                // + INC-002: +150 = 270
                // - OUT-001: -30 = 240
                // - OUT-002: -50 = 190
                // Huidige voorraad: 190
                new Product 
                { 
                    ProductName = "Ribena Blackcurrant 500ml",
                    Description = "Zwarte bessen drankconcentraat",
                    SKU = "RIB-500-001",
                    Category = "Frisdrank",
                    PurchasePrice = 0.80m,
                    SellingPrice = 1.50m,
                    StockQuantity = 190,
                    MinimumStock = 80,
                    SupplierId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-2)
                },
                // Product 4: Schweppes
                // Start: 158
                // + INC-003: +300 = 458
                // - Correction: -8 = 450
                // + levering 8: +200 = 650
                // Huidige voorraad: 650
                new Product 
                { 
                    ProductName = "Schweppes Tonic Water 200ml",
                    Description = "Klassieke tonic water",
                    SKU = "SCH-200-001",
                    Category = "Mixer",
                    PurchasePrice = 0.35m,
                    SellingPrice = 0.75m,
                    StockQuantity = 650,
                    MinimumStock = 60,
                    SupplierId = 2,
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddMonths(-1)
                }
            });
            return list;
        }
    }
}
