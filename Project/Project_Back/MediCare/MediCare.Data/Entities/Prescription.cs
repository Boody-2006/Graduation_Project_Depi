using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Entities
{
    public class Prescription
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int MedicalRecordId { get; set; }

        public MedicalRecord MedicalRecord { get; set; } = null!;

        public List<PrescriptionItem> PrescriptionItems { get; set; } = new();
    }
}
