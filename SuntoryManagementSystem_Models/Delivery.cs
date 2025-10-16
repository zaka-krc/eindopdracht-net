using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuntoryManagementSystem.Models
{
    /// Delivery - Leveringen van producten
    public class Delivery
    {
        /// Unieke identifier voor de levering
        [Key]
        public int DeliveryId { get; set; }

        /// Foreign Key naar Supplier
        [Required(ErrorMessage = "Leverancier is verplicht")]
        [ForeignKey("Supplier")]
        [Display(Name = "Leverancier")]
        public int SupplierId { get; set; }

        /// De leverancier van deze levering
        public Supplier? Supplier { get; set; }

        /// Foreign Key naar Vehicle (optioneel)
        [ForeignKey("Vehicle")]
        [Display(Name = "Voertuig")]
        public int? VehicleId { get; set; }

        /// Het voertuig gebruikt voor deze levering
        public Vehicle? Vehicle { get; set; }

        /// Referentienummer van de levering (bijv. leverbon nummer)
        [Required(ErrorMessage = "Referentienummer is verplicht")]
        [StringLength(50)]
        [Display(Name = "Referentie")]
        public string ReferenceNumber { get; set; } = string.Empty;

        /// Verwachte leverdatum en tijd
        [Required]
        [Display(Name = "Verwachte levering")]
        [DataType(DataType.DateTime)]
        public DateTime ExpectedDeliveryDate { get; set; } = DateTime.Now.AddDays(1);

        /// Werkelijke leverdatum en tijd
        [Display(Name = "Werkelijke levering")]
        [DataType(DataType.DateTime)]
        public DateTime? ActualDeliveryDate { get; set; }

        /// Status van de levering: "Pending", "In Transit", "Delivered", "Cancelled"
        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        /// Totaal bedrag van de levering
        [Required]
        [Column(TypeName = "DECIMAL(10, 2)")]
        [Display(Name = "Totaalbedrag")]
        public decimal TotalAmount { get; set; } = 0.00m;

        /// Is de levering verwerkt in de voorraad?
        [Required]
        [Display(Name = "Verwerkt")]
        public bool IsProcessed { get; set; } = false;

        /// Datum en tijd van aanmaak
        [Required]
        [Display(Name = "Aangemaakt")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// Opmerkingen over de levering
        [StringLength(500)]
        [Display(Name = "Opmerkingen")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; } = string.Empty;

        // NAVIGATION PROPERTIES - Relaties met andere entiteiten

        /// Alle items in deze levering
        public ICollection<DeliveryItem>? DeliveryItems { get; set; }

        public override string ToString()
        {
            return $"{DeliveryId} - Levering {ReferenceNumber} op {ExpectedDeliveryDate:dd-MM-yyyy} ({Status})";
        }

        public static List<Delivery> SeedingData()
        {
            var list = new List<Delivery>();
            list.AddRange(new[]
            {
                new Delivery 
                { 
                    SupplierId = 1,
                    ReferenceNumber = "DEL-2025-001",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(2),
                    Status = "Pending",
                    TotalAmount = 450.00m
                },
                new Delivery 
                { 
                    SupplierId = 1,
                    ReferenceNumber = "DEL-2025-002",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-3),
                    ActualDeliveryDate = DateTime.Now.AddDays(-3).AddHours(2),
                    Status = "Delivered",
                    TotalAmount = 780.50m,
                    IsProcessed = true
                },
                new Delivery 
                { 
                    SupplierId = 2,
                    ReferenceNumber = "DEL-2025-003",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(5),
                    Status = "In Transit",
                    TotalAmount = 320.00m
                }
            });
            return list;
        }
    }
}