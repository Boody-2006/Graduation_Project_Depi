
namespace MediCare.Data.Entities
{
    public class Doctor
    {
        public string Id { get; set; } = string.Empty;

        public decimal ConsultationFee { get; set; }

        public string City { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string ApprovalStatus { get; set; } = "Pending";

        public string? RejectionReason { get; set; }

        public double? AverageRating { get; set; }

        public int SpecializationId { get; set; }
        public Specialization Specialization { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public List<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();
        public List<DoctorVacation> DoctorVacations { get; set; } = new List<DoctorVacation>();
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
