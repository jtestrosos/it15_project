using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manufacturing_system.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        public int? FacilityID { get; set; }

        public bool IsArchived { get; set; } = false;

        [ForeignKey("FacilityID")]
        public Facility? Facility { get; set; }
    }
}
