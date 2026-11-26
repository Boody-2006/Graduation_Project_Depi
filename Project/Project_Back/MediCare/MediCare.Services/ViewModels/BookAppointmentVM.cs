using System.ComponentModel.DataAnnotations;
using MediCare.Services.Factories;

namespace MediCare.Services.ViewModels
{
    /// <summary>Data the patient sends when booking a slot.</summary>
    public class BookAppointmentVM
    {
        [Required(ErrorMessage = "Doctor is required / الطبيب مطلوب")]
        [Display(Name = "Doctor / الطبيب")]
        public string DoctorId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required / التاريخ مطلوب")]
        [DataType(DataType.Date)]
        [Display(Name = "Date / التاريخ")]
        public DateTime AppointmentDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Time is required / الوقت مطلوب")]
        [Display(Name = "Time / الوقت")]
        public TimeOnly StartTime { get; set; }

        [Required(ErrorMessage = "Appointment type is required / نوع الحجز مطلوب")]
        [Display(Name = "Type / نوع الحجز")]
        public string Type { get; set; } = AppointmentTypes.Consultation;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters / الملاحظات لا يمكن أن تتجاوز 500 حرف")]
        [Display(Name = "Notes / ملاحظات")]
        public string? Notes { get; set; }
    }
}
