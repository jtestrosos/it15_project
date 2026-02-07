using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manufacturing_system.Models
{
    public class Component
    {
        [Key]
        public int ComponentID { get; set; }

        [Required]
        [StringLength(50)]
        public string ComponentName { get; set; } = string.Empty;

        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitCost { get; set; }

        [StringLength(3)]
        public string CurrencyCode { get; set; } = "PHP";

        public int MinStockLevel { get; set; }

        [StringLength(100)]
        public string? SupplierInfo { get; set; }
    }
}
