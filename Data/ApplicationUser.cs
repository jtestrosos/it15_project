using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manufacturing_system.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Column(TypeName = "nvarchar(30)")]
        public string FirstName { get; set; } = string.Empty;

        [PersonalData]
        [Column(TypeName = "nvarchar(20)")]
        public string LastName { get; set; } = string.Empty;

        [PersonalData]
        [Column(TypeName = "nvarchar(20)")]
        public string? MiddleName { get; set; }

        // While Identity has Roles, this property is explicitly requested in the dictionary.
        [Column(TypeName = "nvarchar(25)")]
        public string Role { get; set; } = string.Empty;

        public int? FacilityID { get; set; }
        
        [ForeignKey("FacilityID")]
        public manufacturing_system.Models.Facility? Facility { get; set; }

        public bool IsArchived { get; set; } = false;
    }

}
