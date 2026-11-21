namespace MediCare.Data.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
        public string CancelReason { get; set; }
        public string CancelledBy { get; set; }
        public decimal Fee { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        public string PatientId { get; set; }
        public Patient Patient { get; set; }
        public MedicalRecord MedicalRecord { get; set; }
        public int? RescheduledFromId { get; set; }

        public Appointment? RescheduledFrom { get; set; }

        public Appointment? RescheduledTo { get; set; }
    }
}
