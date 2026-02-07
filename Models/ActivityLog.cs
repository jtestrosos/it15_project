using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manufacturing_system.Models
{
    public class ActivityLog
    {
        [Key]
        public int LogID { get; set; }

        public string UserID { get; set; } = string.Empty;

        [ForeignKey("UserID")]
        public manufacturing_system.Data.ApplicationUser User { get; set; } = null!;

        [StringLength(50)]
        public string ActionType { get; set; } = string.Empty;

        [StringLength(200)]
        public string Details { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        [StringLength(15)]
        public string IPAddress { get; set; } = string.Empty;
    }
}
