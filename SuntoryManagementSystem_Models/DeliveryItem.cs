using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuntoryManagementSystem.Models
{
    /// DeliveryItem - Individuele producten in een levering
    /// Dit is een junction/junction-tabel tussen Delivery en Product
    public class DeliveryItem
    {
        /// Unieke identifier voor dit leveringsitem
        [Key]
        public int DeliveryItemId { get; set; }

        /// Foreign Key naar Delivery
        [Required(ErrorMessage = "Levering is verplicht")]
        [ForeignKey("Delivery")]
        public int DeliveryId { get; set; }

        /// De levering waar dit item toe behoort
        public Delivery? Delivery { get; set; }

        /// Foreign Key naar Product
        [Required(ErrorMessage = "Product is verplicht")]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        /// Het product dat in dit item wordt geleverd
        public Product? Product { get; set; }

        /// Aantal stuks van dit product in deze levering
        [Required(ErrorMessage = "Hoeveelheid is verplicht")]
        [Range(1, int.MaxValue, ErrorMessage = "Hoeveelheid moet minimaal 1 zijn")]
        [Display(Name = "Hoeveelheid")]
        public int Quantity { get; set; } = 1;

        /// Prijs per eenheid op moment van levering
        [Required(ErrorMessage = "Prijs per eenheid is verplicht")]
        [Column(TypeName = "DECIMAL(10, 2)")]
        [Display(Name = "Prijs per eenheid")]
        public decimal UnitPrice { get; set; } = 0.00m;

        /// Is dit item al verwerkt in de voorraad?
        [Required]
        [Display(Name = "Verwerkt")]
        public bool IsProcessed { get; set; } = false;

        // SOFT DELETE PROPERTIES
        
        /// Is dit leveringsitem soft-deleted?
        [Required]
        [Display(Name = "Verwijderd")]
        public bool IsDeleted { get; set; } = false;

        /// Datum en tijd van soft delete
        [Display(Name = "Verwijderd op")]
        [DataType(DataType.DateTime)]
        public DateTime? DeletedDate { get; set; }

        // COMPUTED PROPERTIES
        
        /// Totaalprijs voor dit item (Quantity * UnitPrice)
        [NotMapped]
        public decimal Total => Quantity * UnitPrice;

        public override string ToString()
        {
            return $"{DeliveryItemId} - {Quantity}x Product (ID: {ProductId}) @ €{UnitPrice}";
        }

        public static List<DeliveryItem> SeedingData()
        {
            var list = new List<DeliveryItem>();
            list.AddRange(new[]
            {
                // Items voor INC-2025-001 (Delivery ID: 1) - Gepland
                new DeliveryItem 
                { 
                    DeliveryId = 1,
                    ProductId = 1,  // Orangina Original 330ml
                    Quantity = 100,
                    UnitPrice = 0.45m,
                    IsProcessed = false
                },
                new DeliveryItem 
                { 
                    DeliveryId = 1,
                    ProductId = 2,  // Lucozade Energy Original 380ml
                    Quantity = 50,
                    UnitPrice = 0.65m,
                    IsProcessed = false
                },

                // Items voor INC-2025-002 (Delivery ID: 2) - Delivered
                new DeliveryItem 
                { 
                    DeliveryId = 2,
                    ProductId = 1,  // Orangina Original 330ml
                    Quantity = 200,
                    UnitPrice = 0.45m,
                    IsProcessed = true
                },
                new DeliveryItem 
                { 
                    DeliveryId = 2,
                    ProductId = 3,  // Ribena Blackcurrant 500ml
                    Quantity = 150,
                    UnitPrice = 0.80m,
                    IsProcessed = true
                },

                // Items voor INC-2025-003 (Delivery ID: 3) - Gepland
                new DeliveryItem 
                { 
                    DeliveryId = 3,
                    ProductId = 4,  // Schweppes Tonic Water 200ml
                    Quantity = 300,
                    UnitPrice = 0.35m,
                    IsProcessed = false
                },
                new DeliveryItem 
                { 
                    DeliveryId = 3,
                    ProductId = 2,  // Lucozade Energy Original 380ml
                    Quantity = 80,
                    UnitPrice = 0.65m,
                    IsProcessed = false
                },

                // Items voor OUT-2025-001 (Delivery ID: 4) - Gepland
                new DeliveryItem 
                { 
                    DeliveryId = 4,
                    ProductId = 1,  // Orangina Original 330ml
                    Quantity = 50,
                    UnitPrice = 0.90m,  // Selling price
                    IsProcessed = false
                },
                new DeliveryItem 
                { 
                    DeliveryId = 4,
                    ProductId = 3,  // Ribena Blackcurrant 500ml
                    Quantity = 30,
                    UnitPrice = 1.50m,  // Selling price
                    IsProcessed = false
                },

                // Items voor INC-2025-004 (Delivery ID: 5) - Geannuleerd
                new DeliveryItem 
                { 
                    DeliveryId = 5,
                    ProductId = 2,  // Lucozade Energy Original 380ml
                    Quantity = 100,
                    UnitPrice = 0.65m,
                    IsProcessed = false
                },
                new DeliveryItem 
                { 
                    DeliveryId = 5,
                    ProductId = 4,  // Schweppes Tonic Water 200ml
                    Quantity = 150,
                    UnitPrice = 0.35m,
                    IsProcessed = false
                },

                // Items voor OUT-2025-002 (Delivery ID: 6) - Delivered
                new DeliveryItem 
                { 
                    DeliveryId = 6,
                    ProductId = 1,  // Orangina Original 330ml
                    Quantity = 100,
                    UnitPrice = 0.90m,  // Selling price
                    IsProcessed = true
                },
                new DeliveryItem 
                { 
                    DeliveryId = 6,
                    ProductId = 2,  // Lucozade Energy Original 380ml
                    Quantity = 80,
                    UnitPrice = 1.20m,  // Selling price
                    IsProcessed = true
                },
                new DeliveryItem 
                { 
                    DeliveryId = 6,
                    ProductId = 3,  // Ribena Blackcurrant 500ml
                    Quantity = 50,
                    UnitPrice = 1.50m,  // Selling price
                    IsProcessed = true
                }
            });
            return list;
        }
    }
}