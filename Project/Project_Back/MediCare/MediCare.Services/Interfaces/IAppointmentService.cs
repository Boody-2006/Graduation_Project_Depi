using MediCare.Services.ViewModels;

namespace MediCare.Services.Interfaces
{
    /// <summary>
    /// The appointment engine: free slots, booking without conflicts,
    /// and the status flow (pending / confirmed / done / cancelled / rejected / rescheduled).
    /// The userId / doctorId / patientId parameters are the logged-in user's Id,
    /// so a user can only touch their own appointments.
    /// </summary>
    public interface IAppointmentService
    {
        /// <summary>Free slots of a doctor on a day (working hours minus vacations, booked and past slots).</summary>
        Task<List<SlotVM>> GetAvailableSlotsAsync(string doctorId, DateTime date);

        /// <summary>Books a slot for the patient. Refuses double booking, vacations and times outside the schedule.</summary>
        Task<AppointmentResult> BookAppointmentAsync(string patientId, BookAppointmentVM model);

        /// <summary>Doctor accepts a pending request (Pending -> Confirmed).</summary>
        Task<AppointmentResult> AcceptAsync(int appointmentId, string doctorId);

        /// <summary>Doctor rejects a pending request. A reason is required (Pending -> Rejected).</summary>
        Task<AppointmentResult> RejectAsync(int appointmentId, string doctorId, string reason);

        /// <summary>Doctor finishes the visit (Confirmed -> Done).</summary>
        Task<AppointmentResult> CompleteAsync(int appointmentId, string doctorId);

        /// <summary>Patient or doctor of the appointment cancels it (Pending / Confirmed -> Cancelled).</summary>
        Task<AppointmentResult> CancelAsync(int appointmentId, string userId, string? reason = null);

        /// <summary>
        /// Patient moves the appointment to a new slot. The old one becomes Rescheduled and a new
        /// Pending one is created that keeps RescheduledFromId.
        /// </summary>
        Task<AppointmentResult> RescheduleAsync(int appointmentId, string patientId, DateTime newDate, TimeOnly newStartTime);

        /// <summary>One appointment, only if the user is its patient or its doctor; otherwise null.</summary>
        Task<AppointmentVM?> GetByIdAsync(int appointmentId, string userId);

        /// <summary>"My appointments" for a patient.</summary>
        Task<List<AppointmentVM>> GetPatientAppointmentsAsync(string patientId);

        /// <summary>Doctor dashboard: today's confirmed and done appointments.</summary>
        Task<List<AppointmentVM>> GetTodayAppointmentsAsync(string doctorId);

        /// <summary>Doctor dashboard: pending requests waiting for Accept / Reject.</summary>
        Task<List<AppointmentVM>> GetPendingRequestsAsync(string doctorId);
    }
}
