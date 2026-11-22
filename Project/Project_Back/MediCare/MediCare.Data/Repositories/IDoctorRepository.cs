using MediCare.Data.Entities;

namespace MediCare.Data.Repositories
{
    public interface IDoctorRepository : IGenericRepository<Doctor>
    {
        Task<IReadOnlyList<Doctor>> GetApprovedAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Doctor>> SearchAsync(
            string? name = null,
            int? specializationId = null,
            string? city = null,
            bool? isAvailable = null,
            DayOfWeek? availableDay = null,
            TimeSpan? availableAt = null,
            CancellationToken cancellationToken = default);
    }
}
