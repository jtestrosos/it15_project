using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace production_system.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Column(TypeName = "nvarchar(30)")]
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 30 characters.")]
        [RegularExpression(@"^[a-zA-Z\s'\-]+$", ErrorMessage = "First name may only contain letters, spaces, hyphens, and apostrophes.")]
        public string FirstName { get; set; } = string.Empty;

        [PersonalData]
        [Column(TypeName = "nvarchar(20)")]
        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 20 characters.")]
        [RegularExpression(@"^[a-zA-Z\s'\-]+$", ErrorMessage = "Last name may only contain letters, spaces, hyphens, and apostrophes.")]
        public string LastName { get; set; } = string.Empty;

        [PersonalData]
        [Column(TypeName = "nvarchar(20)")]
        [StringLength(20, ErrorMessage = "Middle name cannot exceed 20 characters.")]
        [RegularExpression(@"^[a-zA-Z\s'\-]*$", ErrorMessage = "Middle name may only contain letters, spaces, hyphens, and apostrophes.")]
        public string? MiddleName { get; set; }

        public int? FacilityID { get; set; }
        
        [ForeignKey("FacilityID")]
        public production_system.Models.Facility? Facility { get; set; }

        public bool IsArchived { get; set; } = false;
    }

}

