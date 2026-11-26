namespace MediCare.Services.ViewModels
{
    /// <summary>
    /// An appointment shaped for the views
    /// (doctor dashboard, "My appointments", booking confirmation).
    /// </summary>
    public class AppointmentVM
    {
        public int Id { get; set; }
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? CancelReason { get; set; }
        public string? CancelledBy { get; set; }
        public decimal Fee { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public int? RescheduledFromId { get; set; }
    }
}
