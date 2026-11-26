using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Entities
{
    public class DoctorVacation
    {
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Reason { get; set; }
        public string DoctorId { get; set; }
        public Doctor Doctor { get; set; }
    }
}
