
using MediCare.Services.ViewModels;

namespace MediCare.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<List<DoctorCardVM>> SearchAsync(DoctorFilterVM filter);
    }
}
