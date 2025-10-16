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

        public override string ToString()
        {
            return $"{DeliveryItemId} - {Quantity}x Product (ID: {ProductId}) @ €{UnitPrice}";
        }
    }
}