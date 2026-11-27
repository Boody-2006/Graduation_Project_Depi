namespace MediCare.Services.ViewModels
{
    /// <summary>
    /// Represents a doctor registration record shown in the Admin approval queue.
    /// </summary>
    public class DoctorApprovalItemViewModel
    {
        public string DoctorId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string SpecializationName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public string ApprovalStatus { get; set; } = "Pending";
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// View model for the Admin Doctor Approval page grouped by approval status.
    /// </summary>
    public class AdminDoctorsListViewModel
    {
        public List<DoctorApprovalItemViewModel> PendingDoctors { get; set; } = new();
        public List<DoctorApprovalItemViewModel> ApprovedDoctors { get; set; } = new();
        public List<DoctorApprovalItemViewModel> RejectedDoctors { get; set; } = new();
    }
}
