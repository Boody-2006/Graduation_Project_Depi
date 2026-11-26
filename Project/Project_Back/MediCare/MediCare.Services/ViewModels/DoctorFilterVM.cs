
namespace MediCare.Services.ViewModels
{
    public class DoctorFilterVM
    {
        public string? SearchTerm { get; set; }

        public string? City { get; set; }

        public int? SpecializationId { get; set; }

        public bool? IsAvailable { get; set; }
    }
}
