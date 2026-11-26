using MediCare.Services.ViewModels;

namespace MediCare.Services.Interfaces
{
    /// <summary>
    /// Service for patient appointment booking (FullCalendar slots) and "My Appointments"
    /// management (cancel and reschedule) with strict ownership checks via ICurrentUserService.
    /// </summary>
    public interface IBookingService
    {
        Task<BookingPageViewModel> GetBookingPageAsync(string? preselectedDoctorId, CancellationToken cancellationToken = default);
        Task<List<CalendarSlotEventViewModel>> GetAvailableSlotsAsync(string doctorId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
        Task<(bool Success, string Message)> BookAppointmentAsync(BookAppointmentViewModel model, CancellationToken cancellationToken = default);
        Task<List<PatientAppointmentItemViewModel>> GetMyAppointmentsAsync(CancellationToken cancellationToken = default);
        Task<(bool Success, string Message)> CancelAppointmentAsync(int appointmentId, string? reason, CancellationToken cancellationToken = default);
        Task<(bool Success, string Message)> RescheduleAppointmentAsync(int appointmentId, DateOnly newDate, TimeOnly newTime, CancellationToken cancellationToken = default);
    }
}
