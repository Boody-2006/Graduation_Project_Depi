using MediCare.Data.Entities;
using MediCare.Services.ViewModels;

namespace MediCare.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterPatientAsync(RegisterPatientViewModel model);
        Task<AuthResult> RegisterDoctorAsync(RegisterDoctorViewModel model);
        Task<AuthResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
        Task<IReadOnlyList<Specialization>> GetSpecializationsAsync();
    }
}
