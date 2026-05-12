using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace production_system.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Product name must be between 1 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\(\)&.,]+$", ErrorMessage = "Product name may only contain letters, numbers, spaces, hyphens, parentheses, ampersands, commas, and periods.")]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\(\)&.,;:!?'/]+$", ErrorMessage = "Description contains invalid characters.")]
        public string Description { get; set; } = string.Empty;

        public int? FacilityID { get; set; }

        public bool IsArchived { get; set; } = false;

        [ForeignKey("FacilityID")]
        public Facility? Facility { get; set; }
    }
}

