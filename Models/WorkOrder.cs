using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace production_system.Models
{
    public class WorkOrder
    {
        [Key]
        public int OrderID { get; set; }

        public int PlanID { get; set; }

        [ForeignKey("PlanID")]
        public ProductionPlan Plan { get; set; } = null!;

        public string UserID { get; set; } = string.Empty;

        [ForeignKey("UserID")]
        public production_system.Data.ApplicationUser User { get; set; } = null!;

        [StringLength(100, ErrorMessage = "Order description cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\(\)&.,;:!?'/]+$", ErrorMessage = "Order description contains invalid characters.")]
        public string OrderDescription { get; set; } = string.Empty;

        [StringLength(25)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Status may only contain letters and spaces.")]
        public string Status { get; set; } = "Pending";

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Batch quantity must be at least 1.")]
        public int BatchQuantity { get; set; }

        public int? FacilityID { get; set; }

        [ForeignKey("FacilityID")]
        public Facility? Facility { get; set; }
    }
}

