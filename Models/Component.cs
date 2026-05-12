using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace production_system.Models
{
    public class Component
    {
        [Key]
        public int ComponentID { get; set; }

        [Required(ErrorMessage = "Component name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Component name must be between 1 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\(\)&.,]+$", ErrorMessage = "Component name may only contain letters, numbers, spaces, hyphens, parentheses, ampersands, commas, and periods.")]
        public string ComponentName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\(\)&.,;:!?'/]*$", ErrorMessage = "Description contains invalid characters.")]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 9999999999.99, ErrorMessage = "Unit cost must be a positive value.")]
        public decimal UnitCost { get; set; }

        [StringLength(3)]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be exactly 3 uppercase letters (e.g. PHP, USD).")]
        public string CurrencyCode { get; set; } = "PHP";

        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock level must be 0 or greater.")]
        public int MinStockLevel { get; set; }

        [StringLength(100, ErrorMessage = "Supplier info cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\(\)&.,;:!?'/\#@]*$", ErrorMessage = "Supplier info contains invalid characters.")]
        public string? SupplierInfo { get; set; }
        public bool IsArchived { get; set; } = false;

        public int? FacilityID { get; set; }

        [ForeignKey("FacilityID")]
        public Facility? Facility { get; set; }
    }
}

