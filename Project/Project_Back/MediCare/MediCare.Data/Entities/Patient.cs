
using System.ComponentModel.DataAnnotations.Schema;

namespace MediCare.Data.Entities
{
    public class Patient
    {
        public string Id { get; set; } = string.Empty;

        public DateOnly DateOfBirth { get; set; }
        public int Age
        {
            get
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                int age = today.Year - DateOfBirth.Year;

                if (DateOfBirth > today.AddYears(-age))
                    age--;

                return age;
            }
        }

        public string? BloodType { get; set; }

        public string? MedicalHistory { get; set; }

        public string? Allergies { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
