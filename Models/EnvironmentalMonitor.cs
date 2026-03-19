using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manufacturing_system.Models
{
    public class EnvironmentalMonitor
    {
        [Key]
        public int MonitorID { get; set; }

        public int FacilityID { get; set; }

        [ForeignKey("FacilityID")]
        public Facility Facility { get; set; } = null!;

        [Column(TypeName = "decimal(5,2)")]
        public decimal Temperature { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Humidity { get; set; }

        public DateTime RecordedDate { get; set; }

        [StringLength(25)]
        public string AlertStatus { get; set; } = "Safe";
    }
}
