using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manufacturing_system.Models
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
        public manufacturing_system.Data.ApplicationUser User { get; set; } = null!;

        [StringLength(25)]
        public string TransactionType { get; set; } = "Inbound"; // e.g., Inbound, Outbound, Adjustment

        public int Quantity { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}
