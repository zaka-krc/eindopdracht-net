using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuntoryManagementSystem.Models
{
    // StockAlert - Waarschuwingen voor lage voorraad
    public class StockAlert
    {
        // Unieke identifier voor de voorraad waarschuwing
        [Key]
        public int StockAlertId { get; set; }

        // Foreign Key naar Product
        [Required(ErrorMessage = "Product is verplicht")]
        [ForeignKey("Product")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        // Het product met lage voorraad
        public Product? Product { get; set; }

        // Type waarschuwing: "Low Stock", "Out of Stock", "Critical"
        [Required]
        [StringLength(20)]
        [Display(Name = "Type")]
        public string AlertType { get; set; } = "Low Stock";

        // Status van de waarschuwing: "Active", "Resolved", "Ignored"
        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        // Datum en tijd waarop de waarschuwing is aangemaakt
        [Required]
        [Display(Name = "Aangemaakt")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Datum en tijd waarop de waarschuwing is opgelost
        [Display(Name = "Opgelost")]
        [DataType(DataType.DateTime)]
        public DateTime? ResolvedDate { get; set; }

        // Opmerkingen over de waarschuwing
        [StringLength(500)]
        [Display(Name = "Opmerkingen")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; } = string.Empty;

        // SOFT DELETE PROPERTIES
        
        // Is deze waarschuwing soft-deleted?
        [Required]
        [Display(Name = "Verwijderd")]
        public bool IsDeleted { get; set; } = false;

        // Datum en tijd van soft delete
        [Display(Name = "Verwijderd op")]
        [DataType(DataType.DateTime)]
        public DateTime? DeletedDate { get; set; }

        public override string ToString()
        {
            return $"{StockAlertId} - {AlertType} voor Product ID: {ProductId} - {Status}";
        }

        public static List<StockAlert> SeedingData()
        {
            var list = new List<StockAlert>();
            list.AddRange(new[]
            {
                // Product 2 (Lucozade): Voorraad = 100, Minimum = 120 → Low Stock Alert
                new StockAlert 
                { 
                    ProductId = 2,
                    AlertType = "Low Stock",
                    Status = "Active",
                    CreatedDate = DateTime.Now.AddDays(-2).AddHours(4),
                    Notes = "Voorraad onder minimum na outgoing levering OUT-2025-002. Nieuwe bestelling plaatsen."
                },
                
                // Oude alert voor Product 1 die is opgelost door INC-2025-002
                new StockAlert 
                { 
                    ProductId = 1,
                    AlertType = "Low Stock",
                    Status = "Resolved",
                    CreatedDate = DateTime.Now.AddDays(-5),
                    ResolvedDate = DateTime.Now.AddDays(-3).AddHours(2),
                    Notes = "Opgelost door verwerking van incoming levering INC-2025-002"
                }
            });
            return list;
        }
    }
}