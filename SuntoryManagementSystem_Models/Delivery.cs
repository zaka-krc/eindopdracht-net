using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuntoryManagementSystem.Models
{
    // Delivery - Leveringen van producten (Incoming = van supplier, Outgoing = naar klant)
    public class Delivery
    {
        // Unieke identifier voor de levering
        [Key]
        public int DeliveryId { get; set; }

        // Type levering: "Incoming" (van supplier) of "Outgoing" (naar klant)
        [Required]
        [StringLength(20)]
        [Display(Name = "Type")]
        public string DeliveryType { get; set; } = "Incoming";

        // Foreign Key naar Supplier (voor Incoming deliveries)
        [Display(Name = "Leverancier")]
        public int? SupplierId { get; set; }

        // De leverancier van deze levering (alleen bij Incoming)
        public Supplier? Supplier { get; set; }

        // Foreign Key naar Customer (voor Outgoing deliveries)
        [Display(Name = "Klant")]
        public int? CustomerId { get; set; }

        // De klant voor deze levering (alleen bij Outgoing)
        public Customer? Customer { get; set; }

        // Foreign Key naar Vehicle (optioneel)
        [Display(Name = "Voertuig")]
        public int? VehicleId { get; set; }

        // Het voertuig gebruikt voor deze levering
        public Vehicle? Vehicle { get; set; }

        // Referentienummer van de levering (bijv. leverbon nummer)
        [Required(ErrorMessage = "Referentienummer is verplicht")]
        [StringLength(50)]
        [Display(Name = "Referentie")]
        public string ReferenceNumber { get; set; } = string.Empty;

        // Verwachte leverdatum en tijd
        [Required]
        [Display(Name = "Verwachte levering")]
        [DataType(DataType.DateTime)]
        public DateTime ExpectedDeliveryDate { get; set; } = DateTime.Now.AddDays(1);

        // Werkelijke leverdatum en tijd
        [Display(Name = "Werkelijke levering")]
        [DataType(DataType.DateTime)]
        public DateTime? ActualDeliveryDate { get; set; }

        // Status van de levering: "Gepland", "Delivered", "Geannuleerd"
        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Gepland";

        // Totaal bedrag van de levering
        [Required]
        [Column(TypeName = "DECIMAL(10, 2)")]
        [Display(Name = "Totaalbedrag")]
        public decimal TotalAmount { get; set; } = 0.00m;

        // Is de levering verwerkt in de voorraad?
        [Required]
        [Display(Name = "Verwerkt")]
        public bool IsProcessed { get; set; } = false;

        // Datum en tijd van aanmaak
        [Required]
        [Display(Name = "Aangemaakt")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Opmerkingen over de levering
        [StringLength(500)]
        [Display(Name = "Opmerkingen")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; } = string.Empty;

        // SOFT DELETE PROPERTIES
        
        // Is deze levering soft-deleted?
        [Required]
        [Display(Name = "Verwijderd")]
        public bool IsDeleted { get; set; } = false;

        // Datum en tijd van soft delete
        [Display(Name = "Verwijderd op")]
        [DataType(DataType.DateTime)]
        public DateTime? DeletedDate { get; set; }

        // NAVIGATION PROPERTIES - Relaties met andere entiteiten

        // Alle items in deze levering
        public ICollection<DeliveryItem>? DeliveryItems { get; set; }

        // COMPUTED PROPERTIES
        
        // Partner naam (Leverancier of Klant, afhankelijk van DeliveryType)
        [NotMapped]
        public string PartnerName
        {
            get
            {
                if (DeliveryType == "Incoming")
                {
                    return Supplier?.SupplierName ?? "Geen leverancier";
                }
                else if (DeliveryType == "Outgoing")
                {
                    return Customer?.CustomerName ?? "Geen klant";
                }
                return string.Empty;
            }
        }

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
                // ID 1: INC-2025-001 - Delivered (verwerkt 3 dagen geleden)
                new Delivery 
                { 
                    DeliveryType = "Incoming",
                    SupplierId = 1,  // Suntory Beverage & Food Europe
                    ReferenceNumber = "INC-2025-001",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-3),
                    ActualDeliveryDate = DateTime.Now.AddDays(-3).AddHours(2),
                    Status = "Delivered",
                    TotalAmount = 78.15m,
                    IsProcessed = true,
                    CreatedDate = DateTime.Now.AddDays(-5),
                    Notes = "Levering succesvol ontvangen en verwerkt"
                },
                // ID 2: INC-2025-002 - Delivered (verwerkt 3 dagen geleden)
                new Delivery 
                { 
                    DeliveryType = "Incoming",
                    SupplierId = 1,  // Suntory Beverage & Food Europe
                    ReferenceNumber = "INC-2025-002",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-4),
                    ActualDeliveryDate = DateTime.Now.AddDays(-4).AddHours(2),
                    Status = "Delivered",
                    TotalAmount = 780.50m,
                    IsProcessed = true,
                    CreatedDate = DateTime.Now.AddDays(-6),
                    Notes = "Levering succesvol ontvangen en verwerkt"
                },
                // ID 3: INC-2025-003 - Delivered
                new Delivery 
                { 
                    DeliveryType = "Incoming",
                    SupplierId = 2,  // Nederlandse Dranken Distributie
                    ReferenceNumber = "INC-2025-003",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-7),
                    ActualDeliveryDate = DateTime.Now.AddDays(-7).AddHours(3),
                    Status = "Delivered",
                    TotalAmount = 157.00m,
                    IsProcessed = true,
                    CreatedDate = DateTime.Now.AddDays(-10),
                    Notes = "Levering succesvol verwerkt"
                },
                // ID 4: INC-2025-004 - Geannuleerd
                new Delivery 
                { 
                    DeliveryType = "Incoming",
                    SupplierId = 1,  // Suntory Beverage & Food Europe
                    ReferenceNumber = "INC-2025-004",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-2),
                    Status = "Geannuleerd",
                    TotalAmount = 500.00m,
                    IsProcessed = false,
                    CreatedDate = DateTime.Now.AddDays(-8),
                    Notes = "Geannuleerd wegens leveringsproblemen bij leverancier"
                },
                // ID 5: OUT-2025-001 - Delivered (uitgaande levering)
                new Delivery 
                { 
                    DeliveryType = "Outgoing",
                    CustomerId = 1,  // Albert Heijn Brussel Centrum
                    VehicleId = 1,
                    ReferenceNumber = "OUT-2025-001",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-5),
                    ActualDeliveryDate = DateTime.Now.AddDays(-5).AddHours(4),
                    Status = "Delivered",
                    TotalAmount = 90.65m,
                    IsProcessed = true,
                    CreatedDate = DateTime.Now.AddDays(-6),
                    Notes = "Succesvolle levering naar Albert Heijn"
                },
                // ID 6: OUT-2025-002 - Delivered (uitgaande levering)
                new Delivery 
                { 
                    DeliveryType = "Outgoing",
                    CustomerId = 2,  // Horeca Groothandel De Smet
                    VehicleId = 2,
                    ReferenceNumber = "OUT-2025-002",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-2),
                    ActualDeliveryDate = DateTime.Now.AddDays(-2).AddHours(3),
                    Status = "Delivered",
                    TotalAmount = 890.00m,
                    IsProcessed = true,
                    CreatedDate = DateTime.Now.AddDays(-3),
                    Notes = "Succesvolle levering naar Horeca Groothandel De Smet"
                },
                // ID 7: Nieuwe incoming levering zonder referentie (zoals in screenshot)
                new Delivery 
                { 
                    DeliveryType = "Incoming",
                    SupplierId = 2,  // Nederlandse Dranken Distributie
                    ReferenceNumber = "ee",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-3),
                    ActualDeliveryDate = DateTime.Now.AddDays(-3).AddHours(1),
                    Status = "Delivered",
                    TotalAmount = 2.60m,
                    IsProcessed = true,
                    CreatedDate = DateTime.Now.AddDays(-4),
                    Notes = "Kleine levering"
                },
                // ID 8: Incoming levering met nummer
                new Delivery 
                { 
                    DeliveryType = "Incoming",
                    SupplierId = 2,  // Nederlandse Dranken Distributie
                    ReferenceNumber = "123465",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-3),
                    ActualDeliveryDate = DateTime.Now.AddDays(-3).AddHours(2),
                    Status = "Delivered",
                    TotalAmount = 250.00m,
                    IsProcessed = true,
                    CreatedDate = DateTime.Now.AddDays(-4),
                    Notes = "Levering verwerkt"
                }
            });
            return list;
        }
    }
}