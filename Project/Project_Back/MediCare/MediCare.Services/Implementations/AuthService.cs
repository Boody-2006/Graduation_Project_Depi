using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediCare.Data.Data;
using MediCare.Data.Entities;
using MediCare.Services.Interfaces;
using MediCare.Services.ViewModels;

namespace MediCare.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        public async Task<AuthResult> RegisterPatientAsync(RegisterPatientViewModel model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return AuthResult.Failed("This email is already registered / هذا البريد الإلكتروني مسجل بالفعل.");
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Gender = model.Gender,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                return AuthResult.Failed(createResult.Errors.Select(e => e.Description));
            }

            // Assign Patient role
            await _userManager.AddToRoleAsync(user, "Patient");

            // Add Patient entity
            var patient = new Patient
            {
                Id = user.Id,
                DateOfBirth = DateOnly.FromDateTime(model.DateOfBirth)
            };
            _dbContext.Patients.Add(patient);
            await _dbContext.SaveChangesAsync();

            // Sign in the newly registered patient automatically
            await _signInManager.SignInAsync(user, isPersistent: false);

            return AuthResult.Success();
        }

        public async Task<AuthResult> RegisterDoctorAsync(RegisterDoctorViewModel model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return AuthResult.Failed("This email is already registered / هذا البريد الإلكتروني مسجل بالفعل.");
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Gender = model.Gender,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                return AuthResult.Failed(createResult.Errors.Select(e => e.Description));
            }

            // Assign Doctor role
            await _userManager.AddToRoleAsync(user, "Doctor");

            // Add Doctor entity with Pending status by default
            var doctor = new Doctor
            {
                Id = user.Id,
                SpecializationId = model.SpecializationId,
                ConsultationFee = model.ConsultationFee,
                City = model.City.Trim(),
                Address = model.Address?.Trim(),
                ApprovalStatus = "Pending"
            };
            _dbContext.Doctors.Add(doctor);
            await _dbContext.SaveChangesAsync();

            // Doctor must NOT be signed in; must await Admin/Assistant approval
            return AuthResult.PendingDoctor();
        }

        public async Task<AuthResult> LoginAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return AuthResult.Failed("Invalid email or password / البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                return AuthResult.Failed("Invalid email or password / البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            }

            // If user is Doctor, enforce approval verification
            var isDoctor = await _userManager.IsInRoleAsync(user, "Doctor");
            if (isDoctor)
            {
                var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.Id == user.Id);
                if (doctor == null || string.Equals(doctor.ApprovalStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                {
                    return AuthResult.PendingDoctor();
                }

                if (string.Equals(doctor.ApprovalStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    return AuthResult.RejectedDoctor(doctor.RejectionReason);
                }
            }

            var signInResult = await _signInManager.PasswordSignInAsync(
                user.UserName ?? user.Email!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (signInResult.Succeeded)
            {
                return AuthResult.Success();
            }

            if (signInResult.IsLockedOut)
            {
                return AuthResult.Failed("This account is temporarily locked out / هذا الحساب مقفل مؤقتاً بسبب محاولات الدخول الخاطئة.");
            }

            return AuthResult.Failed("Unable to log in with provided credentials / تعذر تسجيل الدخول بالبيانات المدخلة.");
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
