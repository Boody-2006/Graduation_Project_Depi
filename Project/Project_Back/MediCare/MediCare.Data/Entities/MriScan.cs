using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Entities
{
    public class MriScan
    {
        public int Id { get; set; }

        public string FileUrl { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public string PatientId { get; set; } = string.Empty;

        public Patient Patient { get; set; } = null!;
    }
}
