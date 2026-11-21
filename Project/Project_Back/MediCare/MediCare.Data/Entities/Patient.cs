
namespace MediCare.Data.Entities
{
    public class Patient
    {
        public string Id { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string? BloodType { get; set; }

        public string? MedicalHistory { get; set; }

        public string? Allergies { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
