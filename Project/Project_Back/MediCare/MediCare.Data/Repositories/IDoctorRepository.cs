using MediCare.Data.Entities;

namespace MediCare.Data.Repositories
{
    public interface IDoctorRepository : IGenericRepository<Doctor>
    {
        Task<IReadOnlyList<Doctor>> GetApprovedAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Doctor>> SearchAsync(
            int? specializationId = null,
            string? city = null,
            DayOfWeek? availableDay = null,
            TimeSpan? availableAt = null,
            CancellationToken cancellationToken = default);
    }
}
