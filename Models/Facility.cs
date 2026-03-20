using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace production_system.Models
{
    public class Facility
    {
        [Key]
        public int FacilityID { get; set; }

        [Required]
        [StringLength(50)]
        public string FacilityName { get; set; } = string.Empty;

        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        [StringLength(50)]
        public string APIKeyOpenWeather { get; set; } = string.Empty;

        [StringLength(50)]
        public string APIKeyExchangeRate { get; set; } = string.Empty;

        [StringLength(25)]
        public string SubscriptionStatus { get; set; } = "Active";

        // Navigation property
        public ICollection<production_system.Data.ApplicationUser> Users { get; set; } = new List<production_system.Data.ApplicationUser>();
    }
}

