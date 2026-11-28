using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.ViewModels
{
    public class MedicalRecordIndexViewModel
    {
        public int Id { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string? Symptoms { get; set; }

        public string? Diagnosis { get; set; }

        public string? TreatmentNotes { get; set; }

        public int AppointmentId { get; set; }
    }
}
