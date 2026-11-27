using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.Web.Controllers
{
    /// <summary>
    /// Admin controller for approving or rejecting pending doctor registrations.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminDoctorService _adminDoctorService;

        public AdminController(IAdminDoctorService adminDoctorService)
        {
            _adminDoctorService = adminDoctorService;
        }

        /// <summary>
        /// Lists Pending, Approved, and Rejected doctor accounts.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PendingDoctors(CancellationToken cancellationToken)
        {
            var model = await _adminDoctorService.GetDoctorsForApprovalAsync(cancellationToken);
            return View(model);
        }

        /// <summary>
        /// Approves a pending or previously rejected doctor account so they can sign in.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDoctor(string id, CancellationToken cancellationToken)
        {
            var result = await _adminDoctorService.ApproveDoctorAsync(id, cancellationToken);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(PendingDoctors));
        }

        /// <summary>
        /// Rejects a pending doctor account with a mandatory reason.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectDoctor(string id, string reason, CancellationToken cancellationToken)
        {
            var result = await _adminDoctorService.RejectDoctorAsync(id, reason, cancellationToken);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(PendingDoctors));
        }
    }
}
