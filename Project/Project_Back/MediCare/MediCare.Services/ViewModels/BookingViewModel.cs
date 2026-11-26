using System.ComponentModel.DataAnnotations;

namespace MediCare.Services.ViewModels
{
    /// <summary>
    /// Approved doctor summary used in the Booking doctor selector.
    /// </summary>
    public class BookingDoctorOptionViewModel
    {
        public string DoctorId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
    }

    /// <summary>
    /// FullCalendar event DTO representing an available or booked slot.
    /// </summary>
    public class CalendarSlotEventViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string BackgroundColor { get; set; } = "#e5eff6";
        public string BorderColor { get; set; } = "#1d5f8a";
        public string TextColor { get; set; } = "#14283c";
    }

    /// <summary>
    /// Form payload submitted when a patient books an appointment slot.
    /// </summary>
    public class BookAppointmentViewModel
    {
        [Required(ErrorMessage = "Please select a doctor.")]
        public string DoctorId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please pick an appointment date.")]
        public DateOnly AppointmentDate { get; set; }

        [Required(ErrorMessage = "Please select a time slot.")]
        public TimeOnly StartTime { get; set; }

        [Required]
        public string Type { get; set; } = "Consultation";

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Page view model for the FullCalendar booking screen.
    /// </summary>
    public class BookingPageViewModel
    {
        public string? SelectedDoctorId { get; set; }
        public List<BookingDoctorOptionViewModel> Doctors { get; set; } = new();
        public BookAppointmentViewModel BookingForm { get; set; } = new();
    }

    /// <summary>
    /// Represents an appointment row on the Patient's "My Appointments" page.
    /// </summary>
    public class PatientAppointmentItemViewModel
    {
        public int Id { get; set; }
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? CancelReason { get; set; }
        public string? CancelledBy { get; set; }
        public bool CanModify => Status is "pending" or "confirmed";
    }
}
