using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MediCare.Services.Interfaces;
using MediCare.Services.ViewModels;

namespace MediCare.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model);

            if (result.IsPendingDoctor)
            {
                ModelState.AddModelError(string.Empty, "Your doctor account is pending approval by the clinic administration. / حسابك كطبيب لا يزال قيد المراجعة والاعتماد من قبل إدارة العيادة.");
                return View(model);
            }

            if (result.IsRejectedDoctor)
            {
                var reason = string.IsNullOrWhiteSpace(result.RejectionReason) ? "" : $" Reason: {result.RejectionReason}";
                ModelState.AddModelError(string.Empty, $"Your doctor account has been rejected.{reason} / تم رفض حسابك كطبيب من قبل إدارة العيادة.");
                return View(model);
            }

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/RegisterPatient
        [HttpGet]
        public IActionResult RegisterPatient()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterPatientViewModel());
        }

        // POST: /Account/RegisterPatient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPatient(RegisterPatientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RegisterPatientAsync(model);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            TempData["SuccessMessage"] = "Patient account created successfully! Welcome to MediCare. / تم إنشاء حساب المريض بنجاح! مرحباً بك في ميديكير.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/RegisterDoctor
        [HttpGet]
        public async Task<IActionResult> RegisterDoctor()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            await PopulateSpecializationsViewBagAsync();
            return View(new RegisterDoctorViewModel());
        }

        // POST: /Account/RegisterDoctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterDoctor(RegisterDoctorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSpecializationsViewBagAsync();
                return View(model);
            }

            var result = await _authService.RegisterDoctorAsync(model);
            if (!result.Succeeded && !result.IsPendingDoctor)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                await PopulateSpecializationsViewBagAsync();
                return View(model);
            }

            TempData["InfoMessage"] = "Doctor registration submitted successfully! Your account is pending admin approval before you can log in. / تم تقديم طلب الطبيب بنجاح! حسابك في انتظار موافقة الإدارة قبل تسجيل الدخول.";
            return RedirectToAction("Login");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Account/AdminOnly (Admin-protected endpoint to verify RBAC & AccessDenied redirection)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            return Content("Admin Access Granted: You have verified Admin role authorization.");
        }

        private async Task PopulateSpecializationsViewBagAsync()
        {
            var specializations = await _authService.GetSpecializationsAsync();
            if (specializations != null && specializations.Any())
            {
                ViewBag.Specializations = specializations.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.Name} / {s.NameAr}"
                }).ToList();
            }
            else
            {
                ViewBag.Specializations = new List<SelectListItem>
                {
                    new("Neurology / مخ وأعصاب", "1"),
                    new("Radiology / أشعة", "2"),
                    new("Neurosurgery / جراحة مخ وأعصاب", "3"),
                    new("Internal Medicine / باطنة", "4"),
                    new("Cardiology / قلب", "5"),
                    new("Pediatrics / أطفال", "6"),
                    new("Dermatology / جلدية", "7"),
                    new("Orthopedics / عظام", "8")
                };
            }
        }
    }
}
