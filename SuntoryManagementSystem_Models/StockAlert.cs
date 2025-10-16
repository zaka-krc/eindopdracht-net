using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuntoryManagementSystem.Models
{

    /// StockAlert - Waarschuwingen voor lage voorraad
    public class StockAlert
    {
        /// Unieke identifier voor de voorraad waarschuwing
        [Key]
        public int StockAlertId { get; set; }

        /// Foreign Key naar Product
        [Required(ErrorMessage = "Product is verplicht")]
        [ForeignKey("Product")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        /// Het product met lage voorraad
        public Product? Product { get; set; }

        /// Huidige voorraad op moment van waarschuwing
        [Required]
        [Display(Name = "Huidige voorraad")]
        public int CurrentStock { get; set; } = 0;

        /// Minimale voorraad drempel
        [Required]
        [Display(Name = "Minimum voorraad")]
        public int MinimumStock { get; set; } = 10;

        /// Status van de waarschuwing: "Active", "Resolved", "Ignored"
        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        /// Datum en tijd waarop de waarschuwing is aangemaakt
        [Required]
        [Display(Name = "Aangemaakt")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// Datum en tijd waarop de waarschuwing is opgelost
        [Display(Name = "Opgelost")]
        [DataType(DataType.DateTime)]
        public DateTime? ResolvedDate { get; set; }

        /// Opmerkingen over de waarschuwing
        [StringLength(500)]
        [Display(Name = "Opmerkingen")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{StockAlertId} - Lage voorraad voor Product ID: {ProductId} ({CurrentStock}/{MinimumStock}) - {Status}";
        }

        public static List<StockAlert> SeedingData()
        {
            var list = new List<StockAlert>();
            list.AddRange(new[]
            {
                new StockAlert 
                { 
                    ProductId = 2,
                    CurrentStock = 35,
                    MinimumStock = 40,
                    Status = "Active",
                    Notes = "Voorraad bijna op, nieuwe bestelling plaatsen"
                },
                new StockAlert 
                { 
                    ProductId = 3,
                    CurrentStock = 25,
                    MinimumStock = 30,
                    Status = "Active",
                    Notes = "Urgent: voorraad onder minimum"
                },
                new StockAlert 
                { 
                    ProductId = 1,
                    CurrentStock = 45,
                    MinimumStock = 50,
                    Status = "Resolved",
                    ResolvedDate = DateTime.Now.AddDays(-2),
                    Notes = "Opgelost: nieuwe levering ontvangen"
                }
            });
            return list;
        }
    }
}