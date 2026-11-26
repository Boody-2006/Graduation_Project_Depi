using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediCare.Services.Interfaces;

namespace MediCare.Web.Controllers
{
    /// <summary>
    /// Endpoint used by the booking page (FullCalendar) to load a doctor's free slots.
    /// It only calls the service; it never touches the DbContext.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/slots")]
    public class SlotsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public SlotsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // GET: /api/slots?doctorId=...&date=2026-11-30
        [HttpGet]
        public async Task<IActionResult> GetSlots([FromQuery] string doctorId, [FromQuery] DateTime date)
        {
            var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, date);
            return Ok(slots);
        }
    }
}
