using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manufacturing_system.Models
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
        public manufacturing_system.Data.ApplicationUser User { get; set; } = null!;

        [StringLength(100)]
        public string OrderDescription { get; set; } = string.Empty;

        [StringLength(25)]
        public string Status { get; set; } = "Pending";

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int BatchQuantity { get; set; }
    }
}
