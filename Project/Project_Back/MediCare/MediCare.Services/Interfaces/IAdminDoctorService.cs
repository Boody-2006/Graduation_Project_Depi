using MediCare.Services.ViewModels;

namespace MediCare.Services.Interfaces
{
    /// <summary>
    /// Service for Admin approval/rejection of newly registered doctors.
    /// </summary>
    public interface IAdminDoctorService
    {
        Task<AdminDoctorsListViewModel> GetDoctorsForApprovalAsync(CancellationToken cancellationToken = default);
        Task<(bool Success, string Message)> ApproveDoctorAsync(string doctorId, CancellationToken cancellationToken = default);
        Task<(bool Success, string Message)> RejectDoctorAsync(string doctorId, string reason, CancellationToken cancellationToken = default);
    }
}
