using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace manufacturing_system.Models
{
    public class ArchivedUser
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        public int? FacilityID { get; set; }

        [ForeignKey("FacilityID")]
        public Facility? Facility { get; set; }

        [Column(TypeName = "nvarchar(30)")]
        public string FirstName { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(20)")]
        public string LastName { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(20)")]
        public string? MiddleName { get; set; }

        [Column(TypeName = "nvarchar(25)")]
        public string Role { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(256)")]
        public string? UserName { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string? Email { get; set; }

        public string? PasswordHash { get; set; }

        public DateTime ArchivedDate { get; set; } = DateTime.UtcNow;
    }
}
