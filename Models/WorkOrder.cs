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

        [StringLength(100)]
        public string OrderDescription { get; set; } = string.Empty;

        [StringLength(25)]
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

