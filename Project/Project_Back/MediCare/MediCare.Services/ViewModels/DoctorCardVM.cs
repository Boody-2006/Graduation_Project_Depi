
namespace MediCare.Services.ViewModels
{
    public class DoctorCardVM
    {
        public string DoctorId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string? ProfileImage { get; set; }

        public string SpecializationName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? Address { get; set; }

        public decimal ConsultationFee { get; set; }

        public double? AverageRating { get; set; }
    }
}
