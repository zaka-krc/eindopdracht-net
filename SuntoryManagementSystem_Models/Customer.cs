using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuntoryManagementSystem.Models
{
    /// Customer - Klanten die producten afnemen van Suntory
    public class Customer
    {
        /// Unieke identifier voor de klant
        [Key]
        public int CustomerId { get; set; }

        /// Bedrijfsnaam van de klant
        [Required(ErrorMessage = "Klantnaam is verplicht")]
        [StringLength(100, ErrorMessage = "Klantnaam mag maximaal 100 tekens zijn")]
        [Display(Name = "Klantnaam")]
        public string CustomerName { get; set; } = string.Empty;

        /// Adres van de klant
        [StringLength(200)]
        [Display(Name = "Adres")]
        public string Address { get; set; } = string.Empty;

        /// Postcode van de klant
        [StringLength(20)]
        [Display(Name = "Postcode")]
        public string PostalCode { get; set; } = string.Empty;

        /// Plaats/Stad van de klant
        [StringLength(100)]
        [Display(Name = "Plaats")]
        public string City { get; set; } = string.Empty;

        /// Telefoonnummer voor contact
        [StringLength(20)]
        [Display(Name = "Telefoonnummer")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// E-mailadres van de klant
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Geldig e-mailadres vereist")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        /// Naam van contactpersoon bij de klant
        [StringLength(100)]
        [Display(Name = "Contactpersoon")]
        public string ContactPerson { get; set; } = string.Empty;

        /// Type klant: "Retail", "Wholesale", "HoReCa"
        [Required]
        [StringLength(20)]
        [Display(Name = "Type")]
        public string CustomerType { get; set; } = "Retail";

        /// Status van de klant: "Active", "Inactive"
        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        /// Datum en tijd van aanmaak
        [Required]
        [Display(Name = "Aangemaakt")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// Aanvullende opmerkingen over de klant
        [StringLength(500)]
        [Display(Name = "Opmerkingen")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; } = string.Empty;

        // SOFT DELETE PROPERTIES
        
        /// Is deze klant soft-deleted?
        [Required]
        [Display(Name = "Verwijderd")]
        public bool IsDeleted { get; set; } = false;

        /// Datum en tijd van soft delete
        [Display(Name = "Verwijderd op")]
        [DataType(DataType.DateTime)]
        public DateTime? DeletedDate { get; set; }

        // NAVIGATION PROPERTIES - Relaties met andere entiteiten

        /// Alle leveringen naar deze klant
        public ICollection<Delivery>? Deliveries { get; set; }

        public override string ToString()
        {
            return $"{CustomerId} - {CustomerName} ({CustomerType})";
        }

        public static List<Customer> SeedingData()
        {
            var list = new List<Customer>();
            list.AddRange(new[]
            {
                new Customer 
                { 
                    CustomerName = "Albert Heijn Brussel Centrum",
                    Address = "Nieuwstraat 123",
                    PostalCode = "1000",
                    City = "Brussel",
                    PhoneNumber = "02-1234567",
                    Email = "brussel@ah.be",
                    ContactPerson = "Sophie Vermeulen",
                    CustomerType = "Retail",
                    Status = "Active"
                },
                new Customer 
                { 
                    CustomerName = "Horeca Groothandel De Smet",
                    Address = "Industrielaan 45",
                    PostalCode = "2000",
                    City = "Antwerpen",
                    PhoneNumber = "03-9876543",
                    Email = "info@horecadesmet.be",
                    ContactPerson = "Jan De Smet",
                    CustomerType = "Wholesale",
                    Status = "Active"
                },
                new Customer 
                { 
                    CustomerName = "Restaurant La Belle Vue",
                    Address = "Kasteelstraat 78",
                    PostalCode = "9000",
                    City = "Gent",
                    PhoneNumber = "09-5551234",
                    Email = "contact@labellevue.be",
                    ContactPerson = "Marie Dubois",
                    CustomerType = "HoReCa",
                    Status = "Active"
                }
            });
            return list;
        }
    }
}
