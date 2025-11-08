using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuntoryManagementSystem.Models
{
    /// Delivery - Leveringen van producten (Incoming = van supplier, Outgoing = naar klant)
    public class Delivery
    {
        /// Unieke identifier voor de levering
        [Key]
        public int DeliveryId { get; set; }

        /// Type levering: "Incoming" (van supplier) of "Outgoing" (naar klant)
        [Required]
        [StringLength(20)]
        [Display(Name = "Type")]
        public string DeliveryType { get; set; } = "Incoming";

        /// Foreign Key naar Supplier (voor Incoming deliveries)
        [ForeignKey("Supplier")]
        [Display(Name = "Leverancier")]
        public int? SupplierId { get; set; }

        /// De leverancier van deze levering (alleen bij Incoming)
        public Supplier? Supplier { get; set; }

        /// Foreign Key naar Customer (voor Outgoing deliveries)
        [ForeignKey("Customer")]
        [Display(Name = "Klant")]
        public int? CustomerId { get; set; }

        /// De klant voor deze levering (alleen bij Outgoing)
        public Customer? Customer { get; set; }

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

        /// Status van de levering: "Gepland", "Delivered", "Geannuleerd"
        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Gepland";

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

        // SOFT DELETE PROPERTIES
        
        /// Is deze levering soft-deleted?
        [Required]
        [Display(Name = "Verwijderd")]
        public bool IsDeleted { get; set; } = false;

        /// Datum en tijd van soft delete
        [Display(Name = "Verwijderd op")]
        [DataType(DataType.DateTime)]
        public DateTime? DeletedDate { get; set; }

        // NAVIGATION PROPERTIES - Relaties met andere entiteiten

        /// Alle items in deze levering
        public ICollection<DeliveryItem>? DeliveryItems { get; set; }

        public override string ToString()
        {
            string typeStr = DeliveryType == "Incoming" ? "Inkoop" : "Verkoop";
            return $"{DeliveryId} - {typeStr} {ReferenceNumber} op {ExpectedDeliveryDate:dd-MM-yyyy} ({Status})";
        }

        public static List<Delivery> SeedingData()
        {
            var list = new List<Delivery>();
            list.AddRange(new[]
            {
                // Geplande incoming levering (aangemaakt 3 dagen geleden)
                new Delivery 
                { 
                    DeliveryType = "Incoming",
                    SupplierId = 1,
                    ReferenceNumber = "INC-2025-001",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(2),
                    Status = "Gepland",
                    TotalAmount = 450.00m,
                    IsProcessed = false,
                    CreatedDate = DateTime.Now.AddDays(-3),
                    Notes = "Nieuwe voorraad Orangina en Lucozade"
                },
                // Verwerkte incoming levering (aangemaakt en verwerkt 3 dagen geleden)
                new Delivery 
                { 
                    DeliveryType = "Incoming",
                    SupplierId = 1,
                    ReferenceNumber = "INC-2025-002",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-3),
                    ActualDeliveryDate = DateTime.Now.AddDays(-3).AddHours(2),
                    Status = "Delivered",
                    TotalAmount = 780.50m,
                    IsProcessed = true,
                    CreatedDate = DateTime.Now.AddDays(-5),
                    Notes = "Levering succesvol ontvangen en verwerkt"
                },
                // Geplande incoming levering (aangemaakt 1 dag geleden)
                new Delivery 
                { 
                    DeliveryType = "Incoming",
                    SupplierId = 2,
                    ReferenceNumber = "INC-2025-003",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(5),
                    Status = "Gepland",
                    TotalAmount = 320.00m,
                    IsProcessed = false,
                    CreatedDate = DateTime.Now.AddDays(-1),
                    Notes = "Verwachte levering eind van de week"
                },
                // Geplande outgoing levering naar klant (aangemaakt vandaag)
                new Delivery 
                { 
                    DeliveryType = "Outgoing",
                    CustomerId = 1,
                    VehicleId = 1,
                    ReferenceNumber = "OUT-2025-001",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(1),
                    Status = "Gepland",
                    TotalAmount = 250.00m,
                    IsProcessed = false,
                    CreatedDate = DateTime.Now.AddHours(-5),
                    Notes = "Bestelling voor Albert Heijn Brussel Centrum"
                },
                // Geannuleerde levering (aangemaakt 4 dagen geleden, geannuleerd 2 dagen geleden)
                new Delivery 
                { 
                    DeliveryType = "Incoming",
                    SupplierId = 1,
                    ReferenceNumber = "INC-2025-004",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-1),
                    Status = "Geannuleerd",
                    TotalAmount = 500.00m,
                    IsProcessed = false,
                    CreatedDate = DateTime.Now.AddDays(-4),
                    Notes = "Geannuleerd wegens leveringsproblemen bij leverancier"
                },
                // Verwerkte outgoing levering (aangemaakt 3 dagen geleden, verwerkt 2 dagen geleden)
                new Delivery 
                { 
                    DeliveryType = "Outgoing",
                    CustomerId = 2,
                    VehicleId = 2,
                    ReferenceNumber = "OUT-2025-002",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-2),
                    ActualDeliveryDate = DateTime.Now.AddDays(-2).AddHours(3),
                    Status = "Delivered",
                    TotalAmount = 890.00m,
                    IsProcessed = true,
                    CreatedDate = DateTime.Now.AddDays(-3),
                    Notes = "Succesvolle levering naar Horeca Groothandel De Smet"
                }
            });
            return list;
        }
    }
}