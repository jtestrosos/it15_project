using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manufacturing_system.Models
{
    public class BillOfMaterial
    {
        [Key]
        public int BOMID { get; set; }

        public int ProductID { get; set; }

        [ForeignKey("ProductID")]
        public Product Product { get; set; } = null!;

        public int ComponentID { get; set; }

        [ForeignKey("ComponentID")]
        public Component Component { get; set; } = null!;

        public int QuantityRequired { get; set; }

        [StringLength(25)]
        public string UnitOfMeasure { get; set; } = "Units";

        public bool IsArchived { get; set; }
    }
}
