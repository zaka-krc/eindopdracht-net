
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuntoryManagementSystem.Models
{
    /// Vehicle - Voertuigen gebruikt voor leveringen
    public class Vehicle
    {
        /// Unieke identifier voor het voertuig
        [Key]
        public int VehicleId { get; set; }

        /// Kenteken van het voertuig
        [Required(ErrorMessage = "Kenteken is verplicht")]
        [StringLength(20)]
        [Display(Name = "Kenteken")]
        public string LicensePlate { get; set; } = string.Empty;

        /// Merk van het voertuig
        [Required(ErrorMessage = "Merk is verplicht")]
        [StringLength(50)]
        [Display(Name = "Merk")]
        public string Brand { get; set; } = string.Empty;

        /// Model van het voertuig
        [Required(ErrorMessage = "Model is verplicht")]
        [StringLength(50)]
        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;

        /// Type voertuig: "Van", "Truck", "Lorry"
        [Required]
        [StringLength(20)]
        [Display(Name = "Type")]
        public string VehicleType { get; set; } = "Van";

        /// Maximale laadcapaciteit in kg
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Capaciteit moet positief zijn")]
        [Display(Name = "Capaciteit (kg)")]
        public int Capacity { get; set; } = 1000;

        /// Is het voertuig beschikbaar?
        [Required]
        [Display(Name = "Beschikbaar")]
        public bool IsAvailable { get; set; } = true;

        /// Datum van laatste onderhoud
        [Display(Name = "Laatste onderhoud")]
        [DataType(DataType.Date)]
        public DateTime? LastMaintenanceDate { get; set; }

        /// Datum en tijd van aanmaak
        [Required]
        [Display(Name = "Aangemaakt")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// Opmerkingen over het voertuig
        [StringLength(500)]
        [Display(Name = "Opmerkingen")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; } = string.Empty;

        // NAVIGATION PROPERTIES - Relaties met andere entiteiten

        /// Alle leveringen gedaan met dit voertuig
        public ICollection<Delivery>? Deliveries { get; set; }

        public override string ToString()
        {
            return $"{VehicleId} - {Brand} {Model} ({LicensePlate})";
        }

        public static List<Vehicle> SeedingData()
        {
            var list = new List<Vehicle>();
            list.AddRange(new[]
            {
                new Vehicle 
                { 
                    LicensePlate = "AB-123-CD",
                    Brand = "Mercedes",
                    Model = "Sprinter",
                    VehicleType = "Van",
                    Capacity = 1500,
                    IsAvailable = true,
                    LastMaintenanceDate = DateTime.Now.AddMonths(-2)
                },
                new Vehicle 
                { 
                    LicensePlate = "XY-456-ZW",
                    Brand = "Volvo",
                    Model = "FH16",
                    VehicleType = "Truck",
                    Capacity = 5000,
                    IsAvailable = true,
                    LastMaintenanceDate = DateTime.Now.AddMonths(-1)
                },
                new Vehicle 
                { 
                    LicensePlate = "PQ-789-RS",
                    Brand = "Ford",
                    Model = "Transit",
                    VehicleType = "Van",
                    Capacity = 1200,
                    IsAvailable = false,
                    LastMaintenanceDate = DateTime.Now.AddDays(-5),
                    Notes = "In onderhoud tot eind van de week"
                }
            });
            return list;
        }
    }
}