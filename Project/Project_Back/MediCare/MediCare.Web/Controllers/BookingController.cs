using MediCare.Services.Interfaces;
using MediCare.Services.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers
{
    /// <summary>
    /// Patient-only controller for FullCalendar slot booking and managing own appointments.
    /// </summary>
    [Authorize(Roles = "Patient")]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Displays the FullCalendar booking page where the patient selects a doctor and picks a free slot.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? doctorId, CancellationToken cancellationToken)
        {
            var model = await _bookingService.GetBookingPageAsync(doctorId, cancellationToken);
            return View(model);
        }

        /// <summary>
        /// JSON endpoint consumed by FullCalendar to load available/booked time slots for a doctor.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSlots(string doctorId, DateTime start, DateTime end, CancellationToken cancellationToken)
        {
            var startDate = DateOnly.FromDateTime(start);
            var endDate = DateOnly.FromDateTime(end);
            var slots = await _bookingService.GetAvailableSlotsAsync(doctorId, startDate, endDate, cancellationToken);

            var events = slots.Select(s => new
            {
                id = s.Id,
                title = s.Title,
                start = s.Start,
                end = s.End,
                backgroundColor = s.BackgroundColor,
                borderColor = s.BorderColor,
                textColor = s.TextColor,
                extendedProps = new { isAvailable = s.IsAvailable }
            });

            return Json(events);
        }

        /// <summary>
        /// Books a selected slot for the logged-in patient.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book([Bind(Prefix = "BookingForm")] BookAppointmentViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please select a valid doctor, date, and time slot.";
                return RedirectToAction(nameof(Index), new { doctorId = model.DoctorId });
            }

            var result = await _bookingService.BookAppointmentAsync(model, cancellationToken);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return result.Success
                ? RedirectToAction(nameof(MyAppointments))
                : RedirectToAction(nameof(Index), new { doctorId = model.DoctorId });
        }

        /// <summary>
        /// Displays only the logged-in patient's appointments (ownership enforced in service).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MyAppointments(CancellationToken cancellationToken)
        {
            var appointments = await _bookingService.GetMyAppointmentsAsync(cancellationToken);
            return View(appointments);
        }

        /// <summary>
        /// Cancels an appointment owned by the logged-in patient.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason, CancellationToken cancellationToken)
        {
            var result = await _bookingService.CancelAppointmentAsync(id, reason, cancellationToken);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(MyAppointments));
        }

        /// <summary>
        /// Reschedules an appointment owned by the logged-in patient to a new date and time.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateOnly newDate, TimeOnly newTime, CancellationToken cancellationToken)
        {
            var result = await _bookingService.RescheduleAppointmentAsync(id, newDate, newTime, cancellationToken);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(MyAppointments));
        }
    }
}
