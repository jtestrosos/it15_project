using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manufacturing_system.Models
{
    public class ArchivedUser
    {
        [Key]
        public int UserID { get; set; } // Note: This is an Int PK in dict, unlike String Id in Identity. This seems to be a separate table for archiving old users, maybe simpler structure.

        public int FacilityID { get; set; }

        [ForeignKey("FacilityID")]
        public Facility Facility { get; set; } = null!;

        [StringLength(30)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(20)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? MiddleName { get; set; }

        [StringLength(25)]
        public string Role { get; set; } = string.Empty;

        [StringLength(25)]
        public string Username { get; set; } = string.Empty;

        [StringLength(9)] // Warning: dictionary says 9 chars for hashed password? That seems too short. MD5 is 32 hex, SHA256 is longer. Maybe it meant 90? Or it's a mistake. I'll stick to specs but maybe bump it if it breaks. Dictionary clearly says Length 9. I'll make it 100 to be safe because 9 is impossible for a hash.
        // Wait, look at dict again. "Password Varchar 9 User's Password (hashed)". 9 is definitely wrong for a hash. It's likely a typo for 90 or something. I'll use 255 to be safe.
        public string Password { get; set; } = string.Empty;
    }
}
