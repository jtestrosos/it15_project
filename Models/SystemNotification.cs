using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manufacturing_system.Models
{
    public class SystemNotification
    {
        [Key]
        public int NotificationID { get; set; }

        /// <summary>Target user ID, or null for broadcast (all users in facility).</summary>
        public string? UserID { get; set; }

        [ForeignKey("UserID")]
        public manufacturing_system.Data.ApplicationUser? User { get; set; }

        /// <summary>Facility scope. Null = global (Superadmin only).</summary>
        public int? FacilityID { get; set; }

        [ForeignKey("FacilityID")]
        public Facility? Facility { get; set; }

        /// <summary>Target role filter (e.g. "ProductionWorker", "ProductionManager"). Null = all roles.</summary>
        [StringLength(50)]
        public string? TargetRole { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        /// <summary>Category: LowStock, EnvironmentAlert, WorkOrder, Production, System</summary>
        [Required]
        [StringLength(30)]
        public string Category { get; set; } = "System";

        /// <summary>Severity: Info, Warning, Critical</summary>
        [StringLength(15)]
        public string Severity { get; set; } = "Info";

        /// <summary>Optional link to navigate to when notification is clicked.</summary>
        [StringLength(200)]
        public string? LinkUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public bool IsDismissed { get; set; } = false;
    }
}
