using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MediCare.Data.Entities;
using MediCare.Data.UnitOfWork;
using MediCare.Services.Interfaces;
using MediCare.Services.ViewModels;

namespace MediCare.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IReadOnlyList<Specialization>> GetSpecializationsAsync()
        {
            try
            {
                return await _unitOfWork.Specializations.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve specializations from repository.");
                return Array.Empty<Specialization>();
            }
        }

        public async Task<AuthResult> RegisterPatientAsync(RegisterPatientViewModel model)
        {
            var normalizedEmail = model.Email.Trim().ToLowerInvariant();

            var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingUser != null)
            {
                _logger.LogWarning("Patient registration rejected: email {Email} already exists.", normalizedEmail);
                return AuthResult.Failed("This email is already registered / هذا البريد الإلكتروني مسجل بالفعل.");
            }

            var user = new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Gender = model.Gender,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                _logger.LogWarning("Patient user creation failed for {Email}: {Errors}",
                    normalizedEmail, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return AuthResult.Failed(createResult.Errors.Select(e => e.Description));
            }

            try
            {
                // Assign Patient role
                var roleResult = await _userManager.AddToRoleAsync(user, "Patient");
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to assign Patient role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }

                // Add Patient entity via UnitOfWork
                var patient = new Patient
                {
                    Id = user.Id,
                    DateOfBirth = DateOnly.FromDateTime(model.DateOfBirth)
                };
                await _unitOfWork.Patients.AddAsync(patient);
                await _unitOfWork.SaveChangesAsync();

                // Sign in the newly registered patient automatically
                await _signInManager.SignInAsync(user, isPersistent: false);

                _logger.LogInformation("Patient registered and authenticated successfully: {Email} (UserId: {UserId})", user.Email, user.Id);
                return AuthResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Patient profile for UserId {UserId}. Rolling back user account.", user.Id);
                // Compensating action: remove orphan Identity user to keep database consistent
                await _userManager.DeleteAsync(user);
                return AuthResult.Failed("An error occurred while finalizing patient registration. Please try again / حدث خطأ أثناء إتمام التسجيل، يرجى المحاولة لاحقاً.");
            }
        }

        public async Task<AuthResult> RegisterDoctorAsync(RegisterDoctorViewModel model)
        {
            var normalizedEmail = model.Email.Trim().ToLowerInvariant();

            // Validate that the chosen specialization exists
            var specialization = await _unitOfWork.Specializations.GetByIdAsync(model.SpecializationId);
            if (specialization == null)
            {
                _logger.LogWarning("Doctor registration rejected: invalid specialization ID {SpecId}.", model.SpecializationId);
                return AuthResult.Failed("The selected medical specialization is invalid / التخصص الطبي المختار غير صحيح.");
            }

            var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingUser != null)
            {
                _logger.LogWarning("Doctor registration rejected: email {Email} already exists.", normalizedEmail);
                return AuthResult.Failed("This email is already registered / هذا البريد الإلكتروني مسجل بالفعل.");
            }

            var user = new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Gender = model.Gender,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                _logger.LogWarning("Doctor user creation failed for {Email}: {Errors}",
                    normalizedEmail, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return AuthResult.Failed(createResult.Errors.Select(e => e.Description));
            }

            try
            {
                // Assign Doctor role
                var roleResult = await _userManager.AddToRoleAsync(user, "Doctor");
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to assign Doctor role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }

                // Add Doctor entity with Pending status by default via UnitOfWork
                var doctor = new Doctor
                {
                    Id = user.Id,
                    SpecializationId = model.SpecializationId,
                    ConsultationFee = model.ConsultationFee,
                    City = model.City.Trim(),
                    Address = model.Address?.Trim(),
                    ApprovalStatus = "Pending"
                };
                await _unitOfWork.Doctors.AddAsync(doctor);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Doctor registered successfully with Pending status: {Email} (UserId: {UserId})", user.Email, user.Id);
                // Doctor must NOT be signed in; must await Admin/Assistant approval
                return AuthResult.PendingDoctor();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Doctor profile for UserId {UserId}. Rolling back user account.", user.Id);
                // Compensating action: remove orphan Identity user to keep database consistent
                await _userManager.DeleteAsync(user);
                return AuthResult.Failed("An error occurred while submitting doctor registration. Please try again / حدث خطأ أثناء إرسال طلب الانضمام، يرجى المحاولة لاحقاً.");
            }
        }

        public async Task<AuthResult> LoginAsync(LoginViewModel model)
        {
            var normalizedEmail = model.Email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(normalizedEmail);
            if (user == null)
            {
                _logger.LogWarning("Login failed: account not found for {Email}.", normalizedEmail);
                return AuthResult.Failed("Invalid email or password / البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            }

            // Check if account is locked out before attempting verification
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Login blocked: account {Email} is currently locked out.", normalizedEmail);
                return AuthResult.Failed("This account is temporarily locked out due to multiple failed attempts. Please try again later / هذا الحساب مقفل مؤقتاً بسبب تكرار المحاولات الخاطئة.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                await _userManager.AccessFailedAsync(user);
                _logger.LogWarning("Login failed: incorrect password for {Email}.", normalizedEmail);

                if (await _userManager.IsLockedOutAsync(user))
                {
                    _logger.LogWarning("Account {Email} has just been locked out.", normalizedEmail);
                    return AuthResult.Failed("This account has been temporarily locked out due to multiple failed login attempts / تم قفل هذا الحساب مؤقتاً بسبب تكرار المحاولات الخاطئة.");
                }

                return AuthResult.Failed("Invalid email or password / البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            }

            // If user is a Doctor, enforce administrative approval gate via UnitOfWork
            var isDoctor = await _userManager.IsInRoleAsync(user, "Doctor");
            if (isDoctor)
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(user.Id);
                if (doctor == null || string.Equals(doctor.ApprovalStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Doctor login gated: {Email} is still Pending approval.", normalizedEmail);
                    return AuthResult.PendingDoctor();
                }

                if (string.Equals(doctor.ApprovalStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Doctor login gated: {Email} has been Rejected. Reason: {Reason}",
                        normalizedEmail, doctor.RejectionReason ?? "None");
                    return AuthResult.RejectedDoctor(doctor.RejectionReason);
                }
            }

            // Reset failed access count upon successful credentials & approval check
            await _userManager.ResetAccessFailedCountAsync(user);

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

            _logger.LogInformation("User logged in successfully: {Email} (RememberMe: {RememberMe})", user.Email, model.RememberMe);
            return AuthResult.Success();
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User signed out.");
        }
    }
}
