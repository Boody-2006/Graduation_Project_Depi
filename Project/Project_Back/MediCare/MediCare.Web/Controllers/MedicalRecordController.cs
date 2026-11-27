using MediCare.Services.Interfaces;
using MediCare.Services.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class MedicalRecordController : Controller
    {
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordController(
            IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var records =
                await _medicalRecordService.GetDoctorMedicalRecordsAsync();

            return View(records);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int appointmentId)
        {
            try
            {
                var model =
                    await _medicalRecordService.PrepareCreateAsync(
                        appointmentId);

                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MedicalRecordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _medicalRecordService
                    .CreateWithPrescriptionAsync(model);

                TempData["Success"] =
                    "Medical record and prescription created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(model);
            }
        }
    }
}