using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace production_system.Models
{
    public class InventoryTransaction
    {
        [Key]
        public int TransactionID { get; set; }

        public int ComponentID { get; set; }

        [ForeignKey("ComponentID")]
        public Component Component { get; set; } = null!;

        public int? OrderID { get; set; }

        [ForeignKey("OrderID")]
        public WorkOrder? Order { get; set; }

        public string UserID { get; set; } = string.Empty;

        [ForeignKey("UserID")]
        public production_system.Data.ApplicationUser User { get; set; } = null!;

        [StringLength(25)]
        public string TransactionType { get; set; } = "Inbound"; // e.g., Inbound, Outbound, Adjustment

        [Range(1, int.MaxValue, ErrorMessage = "Transaction quantity must be at least 1.")]
        public int Quantity { get; set; }

        public DateTime TransactionDate { get; set; }

        public int? FacilityID { get; set; }

        [ForeignKey("FacilityID")]
        public Facility? Facility { get; set; }
    }
}

