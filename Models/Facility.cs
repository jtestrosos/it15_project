using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace production_system.Models
{
    public class Facility
    {
        [Key]
        public int FacilityID { get; set; }

        [Required(ErrorMessage = "Facility name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Facility name must be between 1 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\(\)&.,]+$", ErrorMessage = "Facility name may only contain letters, numbers, spaces, hyphens, parentheses, ampersands, commas, and periods.")]
        public string FacilityName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\(\)&.,;:!?'/#]+$", ErrorMessage = "Location contains invalid characters.")]
        public string Location { get; set; } = string.Empty;

        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "API Key may only contain letters and numbers.")]
        public string APIKeyOpenWeather { get; set; } = string.Empty;

        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "API Key may only contain letters and numbers.")]
        public string APIKeyExchangeRate { get; set; } = string.Empty;

        [StringLength(25)]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Subscription status may only contain letters.")]
        public string SubscriptionStatus { get; set; } = "Active";

        // Navigation property
        public ICollection<production_system.Data.ApplicationUser> Users { get; set; } = new List<production_system.Data.ApplicationUser>();
    }
}

