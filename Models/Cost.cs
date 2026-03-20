using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace production_system.Models
{
    public class Cost
    {
        [Key]
        public int CostID { get; set; }

        public int? OrderID { get; set; } // Nullable: Overhead costs may not be linked to a specific order
        [ForeignKey("OrderID")]
        public WorkOrder? Order { get; set; }

        public int? ComponentID { get; set; }

        [ForeignKey("ComponentID")]
        public Component? Component { get; set; }

        [StringLength(25)]
        public string CostType { get; set; } = string.Empty; // Material, Labor, Overhead

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal CurrencyRate { get; set; }

        public DateTime RecordedDate { get; set; }

        public int? FacilityID { get; set; }

        [ForeignKey("FacilityID")]
        public Facility? Facility { get; set; }
    }
}

