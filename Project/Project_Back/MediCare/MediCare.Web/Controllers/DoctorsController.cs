using Microsoft.AspNetCore.Mvc;
using MediCare.Services.Interfaces;
using MediCare.Services.ViewModels;

namespace MediCare.Web.Controllers
{
    /// <summary>
    /// Public doctor listing page with search and filters.
    /// Follows the team rule: Controller -> Service (never the DbContext).
    /// </summary>
    public class DoctorsController : Controller
    {
        private readonly IDoctorService _doctorService;

        // The service is injected by ASP.NET (registered in Program.cs).
        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        /// <summary>
        /// GET /Doctors?SearchTerm=..&amp;City=..&amp;SpecializationId=..&amp;IsAvailable=..
        /// The filter values come from the query string of the filter form.
        /// </summary>
        public async Task<IActionResult> Index(DoctorFilterVM filter)
        {
            var doctors = await _doctorService.SearchAsync(filter);

            // Values the view needs besides the list of doctors.
            ViewBag.Filter = filter;
            ViewBag.Specializations = await _doctorService.GetSpecializationsAsync();

            return View(doctors);
        }
    }
}