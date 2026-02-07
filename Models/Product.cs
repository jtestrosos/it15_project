using System.ComponentModel.DataAnnotations;

namespace manufacturing_system.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(100)]
        public string Description { get; set; } = string.Empty;
    }
}
