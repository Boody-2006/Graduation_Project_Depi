
using Microsoft.AspNetCore.Mvc;
using MediCare.Services.Interfaces;
using MediCare.Services.ViewModels;

namespace MediCare.Web.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        public async Task<IActionResult> Index(DoctorFilterVM filter)
        {
            var doctors = await _doctorService.SearchAsync(filter);

            ViewBag.Filter = filter;

            return View(doctors);
        }
    }
}
