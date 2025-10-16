using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuntoryManagementSystem.Models
{
    /// Product - Producten die worden beheerd in het systeem
    public class Product
    {
        /// Unieke identifier voor het product
        [Key]
        public int ProductId { get; set; }

        /// Foreign Key naar Supplier
        [Required(ErrorMessage = "Leverancier is verplicht")]
        [ForeignKey("Supplier")]
        [Display(Name = "Leverancier")]
        public int SupplierId { get; set; }

        /// De leverancier van dit product
        public Supplier? Supplier { get; set; }

        /// Naam van het product
        [Required(ErrorMessage = "Productnaam is verplicht")]
        [StringLength(100, ErrorMessage = "Productnaam mag maximaal 100 tekens zijn")]
        [Display(Name = "Productnaam")]
        public string ProductName { get; set; } = string.Empty;

        /// Beschrijving van het product
        [StringLength(500)]
        [Display(Name = "Beschrijving")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        /// SKU (Stock Keeping Unit) - unieke productcode
        [Required(ErrorMessage = "SKU is verplicht")]
        [StringLength(50)]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty;

        /// Categorie van het product (bijv. "Frisdrank", "Water", "Energiedrank")
        [Required]
        [StringLength(50)]
        [Display(Name = "Categorie")]
        public string Category { get; set; } = "Frisdrank";

        /// Inkoopprijs per eenheid
        [Required(ErrorMessage = "Inkoopprijs is verplicht")]
        [Column(TypeName = "DECIMAL(10, 2)")]
        [Display(Name = "Inkoopprijs")]
        public decimal PurchasePrice { get; set; } = 0.00m;

        /// Verkoopprijs per eenheid
        [Required(ErrorMessage = "Verkoopprijs is verplicht")]
        [Column(TypeName = "DECIMAL(10, 2)")]
        [Display(Name = "Verkoopprijs")]
        public decimal SellingPrice { get; set; } = 0.00m;

        /// Huidige voorraad
        [Required]
        [Display(Name = "Voorraad")]
        public int StockQuantity { get; set; } = 0;

        /// Minimale voorraad voordat een waarschuwing wordt gegeven
        [Required]
        [Display(Name = "Min. voorraad")]
        public int MinimumStock { get; set; } = 10;

        /// Is het product actief?
        [Required]
        [Display(Name = "Actief")]
        public bool IsActive { get; set; } = true;

        /// Datum en tijd van aanmaak
        [Required]
        [Display(Name = "Aangemaakt")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // NAVIGATION PROPERTIES - Relaties met andere entiteiten

        /// Alle levering items die dit product bevatten
        public ICollection<DeliveryItem>? DeliveryItems { get; set; }

        /// Alle voorraad aanpassingen voor dit product
        public ICollection<StockAdjustment>? StockAdjustments { get; set; }

        /// Alle voorraad waarschuwingen voor dit product
        public ICollection<StockAlert>? StockAlerts { get; set; }

        public override string ToString()
        {
            return $"{ProductId} - {ProductName} (SKU: {SKU}, Voorraad: {StockQuantity})";
        }

        public static List<Product> SeedingData()
        {
            var list = new List<Product>();
            list.AddRange(new[]
            {
                new Product 
                { 
                    ProductName = "Orangina Original 330ml",
                    Description = "Verfrissende sinaasappeldrank met pulp",
                    SKU = "ORG-330-001",
                    Category = "Frisdrank",
                    PurchasePrice = 0.45m,
                    SellingPrice = 0.95m,
                    StockQuantity = 250,
                    MinimumStock = 50,
                    SupplierId = 1
                },
                new Product 
                { 
                    ProductName = "Lucozade Energy Original 380ml",
                    Description = "Energiedrank met glucose",
                    SKU = "LUC-380-001",
                    Category = "Energiedrank",
                    PurchasePrice = 0.65m,
                    SellingPrice = 1.25m,
                    StockQuantity = 180,
                    MinimumStock = 40,
                    SupplierId = 1
                },
                new Product 
                { 
                    ProductName = "Ribena Blackcurrant 500ml",
                    Description = "Zwarte bessen drankconcentraat",
                    SKU = "RIB-500-001",
                    Category = "Frisdrank",
                    PurchasePrice = 0.80m,
                    SellingPrice = 1.50m,
                    StockQuantity = 120,
                    MinimumStock = 30,
                    SupplierId = 1
                },
                new Product 
                { 
                    ProductName = "Schweppes Tonic Water 200ml",
                    Description = "Klassieke tonic water",
                    SKU = "SCH-200-001",
                    Category = "Mixer",
                    PurchasePrice = 0.35m,
                    SellingPrice = 0.75m,
                    StockQuantity = 300,
                    MinimumStock = 60,
                    SupplierId = 2
                }
            });
            return list;
        }
    }
}