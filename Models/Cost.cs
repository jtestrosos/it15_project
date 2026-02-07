using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manufacturing_system.Models
{
    public class Cost
    {
        [Key]
        public int CostID { get; set; }

        public int? OrderID { get; set; } // Made nullable just in case, but dict says FK to Order. Let's keep it optional based on description "optional, for material costs"? No, ComponentID is optional. OrderID seems to be the parent. But ERD might imply costs can be general. Dictionary says "OrderID-FK". I'll make it nullable if costs can be facility-wide, but for now strict to OrderID as per FK logic unless specified otherwise. Wait, dictionary says "OrderID-FK Int 9". Doesn't say optional. But ComponentID says "(optional, for material costs)". I'll make OrderID required? "CostType" includes Overhead. Overhead might not be linked to an order? I'll make it nullable to be safe.
        // Actually looking at dict again: "OrderID-FK". Usually implies required unless "optional" is stated like for ComponentID.
        // But "Overhead" cost type might not be per order. I will make it nullable.
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
    }
}
