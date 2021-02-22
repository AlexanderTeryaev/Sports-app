using Resources;
using System;
using System.ComponentModel.DataAnnotations;

namespace CmsApp.Models
{
    public class VehicleModel
    {
        public int? SportsmanId { get; set; }
        public string IdentNum { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public DateTime? OwnershipDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public int NumberOfPreviousOwners { get; set; }

        [Required]
        public string LicenseNumber { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime? Valid { get; set; }

        [Required]
        public string Type { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int ModelId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime? YearOfProduction { get; set; }
        [Required]
        public double Weight { get; set; }
        [Required]
        public string ChassisNo { get; set; }
        [Required]
        public int TypeOfLicenseId { get; set; }

        [Required]
        public string EngineNo { get; set; }
        [Required]
        public string EngineVolume { get; set; }
        [Required]
        public string EngineProduct { get; set; }
        [Required]
        public double MaxHorsePower { get; set; }
        [Required]
        public string TermsAndConditions { get; set; }
        [Required]
        public int NumberOfImportEntry { get; set; }

        public int UnionId { get; set; }
        public int SeasonId { get; set; }
        public int? VehicleId { get; set; }
    }
}