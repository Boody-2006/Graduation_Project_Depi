using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Entities
{
    public class MriAnalysis
    {
        public int Id { get; set; }

        public string AnalysisResult { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int MriScanId { get; set; }

        public MriScan MriScan { get; set; } = null!;
    }
}
