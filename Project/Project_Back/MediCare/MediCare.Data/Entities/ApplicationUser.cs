
using Microsoft.AspNetCore.Identity;

namespace MediCare.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Gender { get; set; }

        public string? ProfileImage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? AddressId { get; set; }

        public HomeAddress? HomeAddress { get; set; }

        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public Assistant? Assistant { get; set; }
    }
}
