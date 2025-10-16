using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuntoryManagementSystem.Models
{
    /// Supplier - Bedrijven die producten aan Suntory leveren
    public class Supplier
    {
        /// Unieke identifier voor de leverancier
        [Key]                                                                           
        public int SupplierId { get; set; }

        /// Bedrijfsnaam van de leverancier
        [Required(ErrorMessage = "Leveranciersnaam is verplicht")]
        [StringLength(100, ErrorMessage = "Leveranciersnaam mag maximaal 100 tekens zijn")]
        [Display(Name = "Leveranciersnaam")]
        public string SupplierName { get; set; } = string.Empty;

        /// Adres van de leverancier
        [StringLength(200)]
        [Display(Name = "Adres")]
        public string Address { get; set; } = string.Empty;

        /// Postcode van de leverancier
        [StringLength(20)]
        [Display(Name = "Postcode")]
        public string PostalCode { get; set; } = string.Empty;

        /// Plaats/Stad van de leverancier
        [StringLength(100)]
        [Display(Name = "Plaats")]
        public string City { get; set; } = string.Empty;

        /// Telefoonnummer voor contact
        [StringLength(20)]
        [Display(Name = "Telefoonnummer")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// E-mailadres van de leverancier
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Geldig e-mailadres vereist")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        /// Naam van contactpersoon bij de leverancier
        [StringLength(100)]
        [Display(Name = "Contactpersoon")]
        public string ContactPerson { get; set; } = string.Empty;

        /// Status van de leverancier: "Active", "Inactive"
        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        /// Datum en tijd van aanmaak
        [Required]
        [Display(Name = "Aangemaakt")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// Aanvullende opmerkingen over de leverancier
        [StringLength(500)]
        [Display(Name = "Opmerkingen")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; } = string.Empty;

        // NAVIGATION PROPERTIES - Relaties met andere entiteiten

        /// Alle producten van deze leverancier
        public ICollection<Product>? Products { get; set; }

        /// Alle leveringen van deze leverancier
        public ICollection<Delivery>? Deliveries { get; set; }

        public override string ToString()
        {
            return $"{SupplierId} - {SupplierName} ({Status})";
        }

        public static List<Supplier> SeedingData()
        {
            var list = new List<Supplier>();
            list.AddRange(new[]
            {
                new Supplier 
                { 
                    SupplierName = "Suntory Beverage & Food Europe",
                    Address = "Stationsplein 45",
                    PostalCode = "3013AK",
                    City = "Rotterdam",
                    PhoneNumber = "010-1234567",
                    Email = "contact@suntory.eu",
                    ContactPerson = "Jan de Vries",
                    Status = "Active"
                },
                new Supplier 
                { 
                    SupplierName = "Nederlandse Dranken Distributie",
                    Address = "Hoofdweg 123",
                    PostalCode = "1017AB",
                    City = "Amsterdam",
                    PhoneNumber = "020-9876543",
                    Email = "info@ndd.nl",
                    ContactPerson = "Marie Jansen",
                    Status = "Active"
                },
                new Supplier 
                { 
                    SupplierName = "Global Beverage Suppliers",
                    Address = "Industrieweg 78",
                    PostalCode = "5555XY",
                    City = "Eindhoven",
                    PhoneNumber = "040-5551234",
                    Email = "sales@gbs.com",
                    ContactPerson = "Peter Bakker",
                    Status = "Inactive"
                }
            });
            return list;
        }
    }
}