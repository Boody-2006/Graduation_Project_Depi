namespace MediCare.Data.Entities
{
    /// <summary>
    /// The allowed values of Appointment.Status.
    /// Flow: Pending -> Confirmed / Rejected -> Done / Cancelled, and Rescheduled.
    /// Use these constants instead of typing the strings by hand.
    /// </summary>
    public static class AppointmentStatus
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string Done = "Done";
        public const string Cancelled = "Cancelled";
        public const string Rejected = "Rejected";
        public const string Rescheduled = "Rescheduled";

        /// <summary>
        /// Statuses that still occupy the doctor's time slot.
        /// Cancelled, Rejected and Rescheduled appointments free the slot again.
        /// </summary>
        public static readonly IReadOnlyList<string> Active = new[] { Pending, Confirmed, Done };
    }
}
