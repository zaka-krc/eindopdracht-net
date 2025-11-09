using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuntoryManagementSystem.Models
{
    // StockAdjustment - Aanpassingen in productvoorraad
    public class StockAdjustment
    {
        // Unieke identifier voor de voorraad aanpassing
        [Key]
        public int StockAdjustmentId { get; set; }

        // Foreign Key naar Product
        [Required(ErrorMessage = "Product is verplicht")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        // Het product waarvan de voorraad is aangepast
        public Product? Product { get; set; }

        // Type aanpassing: "Addition", "Removal", "Correction", "Damage", "Theft"
        [Required(ErrorMessage = "Type aanpassing is verplicht")]
        [StringLength(20)]
        [Display(Name = "Type aanpassing")]
        public string AdjustmentType { get; set; } = "Correction";

        // Hoeveelheid aanpassing (positief of negatief)
        [Required]
        [Display(Name = "Hoeveelheid")]
        public int QuantityChange { get; set; } = 0;

        // Voorraad voor de aanpassing
        [Required]
        [Display(Name = "Voorraad voor")]
        public int PreviousQuantity { get; set; } = 0;

        // Voorraad na de aanpassing
        [Required]
        [Display(Name = "Voorraad na")]
        public int NewQuantity { get; set; } = 0;

        // Reden voor de aanpassing
        [Required(ErrorMessage = "Reden is verplicht")]
        [StringLength(500)]
        [Display(Name = "Reden")]
        [DataType(DataType.MultilineText)]
        public string Reason { get; set; } = string.Empty;

        // Datum en tijd van de aanpassing
        [Required]
        [Display(Name = "Datum aanpassing")]
        [DataType(DataType.DateTime)]
        public DateTime AdjustmentDate { get; set; } = DateTime.Now;

        // Wie heeft de aanpassing gedaan
        [StringLength(100)]
        [Display(Name = "Aangepast door")]
        public string AdjustedBy { get; set; } = string.Empty;

        // SOFT DELETE PROPERTIES
        
        // Is deze aanpassing soft-deleted?
        [Required]
        [Display(Name = "Verwijderd")]
        public bool IsDeleted { get; set; } = false;

        // Datum en tijd van soft delete
        [Display(Name = "Verwijderd op")]
        [DataType(DataType.DateTime)]
        public DateTime? DeletedDate { get; set; }

        public override string ToString()
        {
            return $"{StockAdjustmentId} - {AdjustmentType} van {QuantityChange} stuks (Product ID: {ProductId}) op {AdjustmentDate:dd-MM-yyyy HH:mm}";
        }

        public static List<StockAdjustment> SeedingData()
        {
            var list = new List<StockAdjustment>();
            list.AddRange(new[]
            {
                // Aanpassing door verwerking van INC-2025-001 (3 dagen geleden)
                new StockAdjustment 
                { 
                    ProductId = 1,  // Orangina
                    AdjustmentType = "Addition",
                    QuantityChange = 100,
                    PreviousQuantity = 200,
                    NewQuantity = 300,
                    Reason = "Incoming levering INC-2025-001 verwerkt",
                    AdjustedBy = "Warehouse Manager",
                    AdjustmentDate = DateTime.Now.AddDays(-3).AddHours(2)
                },
                new StockAdjustment 
                { 
                    ProductId = 2,  // Lucozade
                    AdjustmentType = "Addition",
                    QuantityChange = 50,
                    PreviousQuantity = 195,
                    NewQuantity = 245,
                    Reason = "Incoming levering INC-2025-001 verwerkt",
                    AdjustedBy = "Warehouse Manager",
                    AdjustmentDate = DateTime.Now.AddDays(-3).AddHours(2).AddMinutes(1)
                },

                // Aanpassing door verwerking van INC-2025-002 (4 dagen geleden)
                new StockAdjustment 
                { 
                    ProductId = 1,  // Orangina
                    AdjustmentType = "Addition",
                    QuantityChange = 200,
                    PreviousQuantity = 300,
                    NewQuantity = 500,
                    Reason = "Incoming levering INC-2025-002 verwerkt",
                    AdjustedBy = "Warehouse Manager",
                    AdjustmentDate = DateTime.Now.AddDays(-4).AddHours(2)
                },
                new StockAdjustment 
                { 
                    ProductId = 3,  // Ribena
                    AdjustmentType = "Addition",
                    QuantityChange = 150,
                    PreviousQuantity = 120,
                    NewQuantity = 270,
                    Reason = "Incoming levering INC-2025-002 verwerkt",
                    AdjustedBy = "Warehouse Manager",
                    AdjustmentDate = DateTime.Now.AddDays(-4).AddHours(2).AddMinutes(1)
                },

                // Aanpassing door verwerking van INC-2025-003 (7 dagen geleden)
                new StockAdjustment 
                { 
                    ProductId = 4,  // Schweppes
                    AdjustmentType = "Addition",
                    QuantityChange = 300,
                    PreviousQuantity = 158,
                    NewQuantity = 458,
                    Reason = "Incoming levering INC-2025-003 verwerkt",
                    AdjustedBy = "Peter de Vries",
                    AdjustmentDate = DateTime.Now.AddDays(-7).AddHours(3)
                },
                new StockAdjustment 
                { 
                    ProductId = 2,  // Lucozade
                    AdjustmentType = "Addition",
                    QuantityChange = 80,
                    PreviousQuantity = 245,
                    NewQuantity = 325,
                    Reason = "Incoming levering INC-2025-003 verwerkt",
                    AdjustedBy = "Peter de Vries",
                    AdjustmentDate = DateTime.Now.AddDays(-7).AddHours(3).AddMinutes(1)
                },

                // Handmatige correctie - Schade (1 week geleden)
                new StockAdjustment 
                { 
                    ProductId = 2,  // Lucozade
                    AdjustmentType = "Damage",
                    QuantityChange = -15,
                    PreviousQuantity = 325,
                    NewQuantity = 310,
                    Reason = "Beschadigde producten door transport - pallet omgevallen",
                    AdjustedBy = "Warehouse Manager",
                    AdjustmentDate = DateTime.Now.AddDays(-7).AddHours(10)
                },

                // Aanpassing door verwerking van OUT-2025-001 (5 dagen geleden)
                new StockAdjustment 
                { 
                    ProductId = 1,  // Orangina
                    AdjustmentType = "Removal",
                    QuantityChange = -50,
                    PreviousQuantity = 500,
                    NewQuantity = 450,
                    Reason = "Outgoing levering OUT-2025-001 verwerkt",
                    AdjustedBy = "Peter de Vries",
                    AdjustmentDate = DateTime.Now.AddDays(-5).AddHours(4)
                },
                new StockAdjustment 
                { 
                    ProductId = 3,  // Ribena
                    AdjustmentType = "Removal",
                    QuantityChange = -30,
                    PreviousQuantity = 270,
                    NewQuantity = 240,
                    Reason = "Outgoing levering OUT-2025-001 verwerkt",
                    AdjustedBy = "Peter de Vries",
                    AdjustmentDate = DateTime.Now.AddDays(-5).AddHours(4).AddMinutes(1)
                },

                // Handmatige correctie (5 dagen geleden)
                new StockAdjustment 
                { 
                    ProductId = 4,  // Schweppes
                    AdjustmentType = "Correction",
                    QuantityChange = -8,
                    PreviousQuantity = 458,
                    NewQuantity = 450,
                    Reason = "Tellingsverschil na inventarisatie - correctie naar werkelijke voorraad",
                    AdjustedBy = "Admin",
                    AdjustmentDate = DateTime.Now.AddDays(-5).AddHours(14)
                },

                // Aanpassing door verwerking van OUT-2025-002 (2 dagen geleden)
                new StockAdjustment 
                { 
                    ProductId = 1,  // Orangina
                    AdjustmentType = "Removal",
                    QuantityChange = -100,
                    PreviousQuantity = 450,
                    NewQuantity = 350,
                    Reason = "Outgoing levering OUT-2025-002 verwerkt",
                    AdjustedBy = "Peter de Vries",
                    AdjustmentDate = DateTime.Now.AddDays(-2).AddHours(3)
                },
                new StockAdjustment 
                { 
                    ProductId = 2,  // Lucozade
                    AdjustmentType = "Removal",
                    QuantityChange = -80,
                    PreviousQuantity = 310,
                    NewQuantity = 230,
                    Reason = "Outgoing levering OUT-2025-002 verwerkt",
                    AdjustedBy = "Peter de Vries",
                    AdjustmentDate = DateTime.Now.AddDays(-2).AddHours(3).AddMinutes(1)
                },
                new StockAdjustment 
                { 
                    ProductId = 3,  // Ribena
                    AdjustmentType = "Removal",
                    QuantityChange = -50,
                    PreviousQuantity = 240,
                    NewQuantity = 190,
                    Reason = "Outgoing levering OUT-2025-002 verwerkt",
                    AdjustedBy = "Peter de Vries",
                    AdjustmentDate = DateTime.Now.AddDays(-2).AddHours(3).AddMinutes(2)
                },

                // Aanpassing door verwerking van levering 7 ("ee") (3 dagen geleden)
                new StockAdjustment 
                { 
                    ProductId = 1,  // Orangina
                    AdjustmentType = "Addition",
                    QuantityChange = 5,
                    PreviousQuantity = 350,
                    NewQuantity = 355,
                    Reason = "Incoming levering ee verwerkt",
                    AdjustedBy = "Warehouse Manager",
                    AdjustmentDate = DateTime.Now.AddDays(-3).AddHours(1)
                },

                // Aanpassing door verwerking van levering 8 ("123465") (3 dagen geleden)
                new StockAdjustment 
                { 
                    ProductId = 2,  // Lucozade
                    AdjustmentType = "Addition",
                    QuantityChange = 100,
                    PreviousQuantity = 230,
                    NewQuantity = 330,
                    Reason = "Incoming levering 123465 verwerkt",
                    AdjustedBy = "Warehouse Manager",
                    AdjustmentDate = DateTime.Now.AddDays(-3).AddHours(2)
                },
                new StockAdjustment 
                { 
                    ProductId = 4,  // Schweppes
                    AdjustmentType = "Addition",
                    QuantityChange = 200,
                    PreviousQuantity = 450,
                    NewQuantity = 650,
                    Reason = "Incoming levering 123465 verwerkt",
                    AdjustedBy = "Warehouse Manager",
                    AdjustmentDate = DateTime.Now.AddDays(-3).AddHours(2).AddMinutes(1)
                }
            });
            return list;
        }
    }
}
