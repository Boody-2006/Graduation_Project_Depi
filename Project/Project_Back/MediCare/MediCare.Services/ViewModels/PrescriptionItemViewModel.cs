using System.ComponentModel.DataAnnotations;

namespace MediCare.Services.ViewModels
{
    public class PrescriptionItemViewModel
    {
        [Required(ErrorMessage = "Medication name is required.")]
        [StringLength(200)]
        public string MedicationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dosage is required.")]
        [StringLength(200)]
        public string Dosage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Frequency is required.")]
        [StringLength(200)]
        public string Frequency { get; set; } = string.Empty;

        [Required(ErrorMessage = "Duration is required.")]
        [StringLength(200)]
        public string Duration { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Instructions { get; set; }
    }
}