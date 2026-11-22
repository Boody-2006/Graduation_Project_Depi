using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Entities
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        public string? Symptoms { get; set; }
        public string? Diagnosis { get; set; }
        public string? TreatmentNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
    }
}
