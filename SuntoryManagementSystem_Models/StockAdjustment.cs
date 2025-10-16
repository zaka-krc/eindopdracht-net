using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuntoryManagementSystem.Models
{
    /// StockAdjustment - Aanpassingen in productvoorraad
    public class StockAdjustment
    {
        /// Unieke identifier voor de voorraad aanpassing
        [Key]
        public int StockAdjustmentId { get; set; }

        /// Foreign Key naar Product
        [Required(ErrorMessage = "Product is verplicht")]
        [ForeignKey("Product")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        /// Het product waarvan de voorraad is aangepast
        public Product? Product { get; set; }

        /// Type aanpassing: "Addition", "Removal", "Correction", "Damage", "Theft"
        [Required(ErrorMessage = "Type aanpassing is verplicht")]
        [StringLength(20)]
        [Display(Name = "Type aanpassing")]
        public string AdjustmentType { get; set; } = "Correction";

        /// Hoeveelheid aanpassing (positief of negatief)
        [Required]
        [Display(Name = "Hoeveelheid")]
        public int QuantityChange { get; set; } = 0;

        /// Voorraad voor de aanpassing
        [Required]
        [Display(Name = "Voorraad voor")]
        public int PreviousQuantity { get; set; } = 0;

        /// Voorraad na de aanpassing
        [Required]
        [Display(Name = "Voorraad na")]
        public int NewQuantity { get; set; } = 0;

        /// Reden voor de aanpassing
        [Required(ErrorMessage = "Reden is verplicht")]
        [StringLength(500)]
        [Display(Name = "Reden")]
        [DataType(DataType.MultilineText)]
        public string Reason { get; set; } = string.Empty;

        /// Datum en tijd van de aanpassing
        [Required]
        [Display(Name = "Datum aanpassing")]
        [DataType(DataType.DateTime)]
        public DateTime AdjustmentDate { get; set; } = DateTime.Now;

        /// Wie heeft de aanpassing gedaan
        [StringLength(100)]
        [Display(Name = "Aangepast door")]
        public string AdjustedBy { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{StockAdjustmentId} - {AdjustmentType} van {QuantityChange} stuks (Product ID: {ProductId}) op {AdjustmentDate:dd-MM-yyyy HH:mm}";
        }

        public static List<StockAdjustment> SeedingData()
        {
            var list = new List<StockAdjustment>();
            list.AddRange(new[]
            {
                new StockAdjustment 
                { 
                    ProductId = 1,
                    AdjustmentType = "Addition",
                    QuantityChange = 50,
                    PreviousQuantity = 200,
                    NewQuantity = 250,
                    Reason = "Nieuwe levering ontvangen",
                    AdjustedBy = "Admin"
                },
                new StockAdjustment 
                { 
                    ProductId = 2,
                    AdjustmentType = "Damage",
                    QuantityChange = -15,
                    PreviousQuantity = 195,
                    NewQuantity = 180,
                    Reason = "Beschadigde producten door transport",
                    AdjustedBy = "Admin"
                },
                new StockAdjustment 
                { 
                    ProductId = 3,
                    AdjustmentType = "Correction",
                    QuantityChange = -5,
                    PreviousQuantity = 125,
                    NewQuantity = 120,
                    Reason = "Tellingsverschil na inventarisatie",
                    AdjustedBy = "Warehouse Manager"
                }
            });
            return list;
        }
    }
}