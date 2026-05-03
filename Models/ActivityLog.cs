using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace production_system.Models
{
    public class ActivityLog
    {
        [Key]
        public int LogID { get; set; }

        public string UserID { get; set; } = string.Empty;

        [ForeignKey("UserID")]
        public production_system.Data.ApplicationUser User { get; set; } = null!;

        [StringLength(50)]
        public string ActionType { get; set; } = string.Empty;

        [StringLength(500)]
        public string Details { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        [StringLength(15)]
        public string IPAddress { get; set; } = string.Empty;

        public int? FacilityID { get; set; }

        [ForeignKey("FacilityID")]
        public Facility? Facility { get; set; }

        /// <summary>
        /// Log category: Security, System, or Audit
        /// </summary>
        [StringLength(20)]
        public string LogCategory { get; set; } = "System";

        /// <summary>
        /// Severity level: Info, Warning, Critical
        /// </summary>
        [StringLength(20)]
        public string Severity { get; set; } = "Info";

        /// <summary>
        /// The entity type affected (e.g. "Product", "User", "WorkOrder")
        /// </summary>
        [StringLength(50)]
        public string? EntityType { get; set; }

        /// <summary>
        /// The ID of the affected entity for traceability
        /// </summary>
        [StringLength(100)]
        public string? EntityId { get; set; }
    }

    /// <summary>
    /// Constants for log categories
    /// </summary>
    public static class LogCategory
    {
        public const string Security = "Security";
        public const string System = "System";
        public const string Audit = "Audit";
    }

    /// <summary>
    /// Constants for log severity levels
    /// </summary>
    public static class LogSeverity
    {
        public const string Info = "Info";
        public const string Warning = "Warning";
        public const string Critical = "Critical";
    }
}
