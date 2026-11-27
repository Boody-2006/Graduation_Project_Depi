using System.ComponentModel.DataAnnotations;

namespace MediCare.Services.ViewModels
{
    public class MedicalRecordViewModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int AppointmentId { get; set; }

        public string? Symptoms { get; set; }

        [Required(ErrorMessage = "Diagnosis is required.")]
        [StringLength(2000)]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? TreatmentNotes { get; set; }

        [MinLength(1, ErrorMessage = "Add at least one medicine.")]
        public List<PrescriptionItemViewModel> Medicines { get; set; } = new();
    }
}