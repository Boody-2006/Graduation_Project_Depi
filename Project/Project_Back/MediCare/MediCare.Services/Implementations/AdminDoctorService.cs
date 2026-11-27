using MediCare.Data.Entities;
using MediCare.Data.UnitOfWork;
using MediCare.Services.Interfaces;
using MediCare.Services.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace MediCare.Services.Implementations
{
    /// <summary>
    /// Manages Admin approval and rejection of doctor accounts via Unit of Work and UserManager.
    /// </summary>
    public class AdminDoctorService : IAdminDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminDoctorService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<AdminDoctorsListViewModel> GetDoctorsForApprovalAsync(CancellationToken cancellationToken = default)
        {
            var doctors = await _unitOfWork.Doctors.GetAllAsync(cancellationToken);
            var specializations = await _unitOfWork.Specializations.GetAllAsync(cancellationToken);
            var specMap = specializations.ToDictionary(s => s.Id, s => s.Name);

            var items = new List<DoctorApprovalItemViewModel>();
            foreach (var doc in doctors)
            {
                var user = await _userManager.FindByIdAsync(doc.Id);
                items.Add(new DoctorApprovalItemViewModel
                {
                    DoctorId = doc.Id,
                    FullName = user != null ? $"Dr. {user.FirstName} {user.LastName}".Trim() : "Unknown Doctor",
                    Email = user?.Email ?? string.Empty,
                    PhoneNumber = user?.PhoneNumber,
                    SpecializationName = specMap.TryGetValue(doc.SpecializationId, out var specName) ? specName : "General",
                    City = doc.City,
                    ConsultationFee = doc.ConsultationFee,
                    ApprovalStatus = doc.ApprovalStatus,
                    RejectionReason = doc.RejectionReason,
                    CreatedAt = user?.CreatedAt ?? DateTime.UtcNow
                });
            }

            return new AdminDoctorsListViewModel
            {
                PendingDoctors = items.Where(d => d.ApprovalStatus == "Pending").OrderByDescending(d => d.CreatedAt).ToList(),
                ApprovedDoctors = items.Where(d => d.ApprovalStatus == "Approved").OrderBy(d => d.FullName).ToList(),
                RejectedDoctors = items.Where(d => d.ApprovalStatus == "Rejected").OrderByDescending(d => d.CreatedAt).ToList()
            };
        }

        public async Task<(bool Success, string Message)> ApproveDoctorAsync(string doctorId, CancellationToken cancellationToken = default)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId, cancellationToken);
            if (doctor == null)
                return (false, "Doctor account not found.");

            doctor.ApprovalStatus = "Approved";
            doctor.RejectionReason = null;

            await _unitOfWork.Doctors.UpdateAsync(doctor);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var user = await _userManager.FindByIdAsync(doctorId);
            var name = user != null ? $"Dr. {user.FirstName} {user.LastName}" : "Doctor";
            return (true, $"{name} has been approved and can now log in.");
        }

        public async Task<(bool Success, string Message)> RejectDoctorAsync(string doctorId, string reason, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return (false, "Please provide a reason for rejecting this doctor.");

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId, cancellationToken);
            if (doctor == null)
                return (false, "Doctor account not found.");

            doctor.ApprovalStatus = "Rejected";
            doctor.RejectionReason = reason.Trim();

            await _unitOfWork.Doctors.UpdateAsync(doctor);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var user = await _userManager.FindByIdAsync(doctorId);
            var name = user != null ? $"Dr. {user.FirstName} {user.LastName}" : "Doctor";
            return (true, $"{name} has been rejected.");
        }
    }
}
