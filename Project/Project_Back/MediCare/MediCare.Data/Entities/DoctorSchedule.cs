using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Entities
{
    public class DoctorSchedule
    {
        public int Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotMinutes { get; set; }
        public bool IsActive { get; set; }
        public string DoctorId { get; set; }
        public Doctor Doctor { get; set; }
    }
}
