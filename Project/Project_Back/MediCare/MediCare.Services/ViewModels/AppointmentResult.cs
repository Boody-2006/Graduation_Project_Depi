namespace MediCare.Services.ViewModels
{
    /// <summary>
    /// Result of an appointment operation (book, accept, reject, cancel ...).
    /// Same idea as AuthResult: check Succeeded, otherwise show Errors.
    /// </summary>
    public class AppointmentResult
    {
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; } = new();

        /// <summary>Id of the appointment that was created or changed (useful for notifications).</summary>
        public int? AppointmentId { get; set; }

        public static AppointmentResult Success(int? appointmentId = null)
        {
            return new AppointmentResult { Succeeded = true, AppointmentId = appointmentId };
        }

        public static AppointmentResult Failed(params string[] errors)
        {
            return new AppointmentResult
            {
                Succeeded = false,
                Errors = errors.ToList()
            };
        }
    }
}
