
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

        public ApplicationUser User { get; set; } = null!;
    }
}
