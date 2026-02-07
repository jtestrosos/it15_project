using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manufacturing_system.Models
{
    public class ProductionPlan
    {
        [Key]
        public int PlanID { get; set; }

        public int ProductID { get; set; }

        [ForeignKey("ProductID")]
        public Product Product { get; set; } = null!;

        public string UserID { get; set; } = string.Empty;

        [ForeignKey("UserID")]
        public manufacturing_system.Data.ApplicationUser User { get; set; } = null!;

        public int BatchQuantity { get; set; }

        [DataType(DataType.Date)]
        public DateTime PlannedStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime PlannedEndDate { get; set; }

        [StringLength(25)]
        public string Status { get; set; } = "Draft";
    }
}
