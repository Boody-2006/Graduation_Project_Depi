using MediCare.Data.Data;
using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data.Repositories
{
    public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<IReadOnlyList<Doctor>> GetApprovedAsync(CancellationToken cancellationToken = default)
        {
            return await GetApprovedDoctorsQuery().ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Doctor>> SearchAsync(
            int? specializationId = null,
            string? city = null,
            DayOfWeek? availableDay = null,
            TimeOnly? availableAt = null,
            CancellationToken cancellationToken = default)
        {
            if (availableDay.HasValue != availableAt.HasValue)
            {
                throw new ArgumentException("Provide both an availability day and time, or neither.");
            }

            var query = GetApprovedDoctorsQuery();

            if (specializationId.HasValue)
            {
                query = query.Where(doctor => doctor.SpecializationId == specializationId.Value);
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                var cityPattern = $"%{EscapeLikePattern(city.Trim())}%";
                query = query.Where(doctor => EF.Functions.Like(doctor.City, cityPattern, "\\"));
            }

            if (availableDay.HasValue && availableAt.HasValue)
            {
                var day = availableDay.Value;
                var time = availableAt.Value;
                var availableDoctorIds = _context.Set<DoctorSchedule>()
                    .Where(schedule =>
                        schedule.IsActive &&                         schedule.DayOfWeek == day &&
                        schedule.StartTime <= time &&
                        schedule.EndTime > time)
                    .Select(schedule => schedule.DoctorId);

                query = query.Where(doctor => availableDoctorIds.Contains(doctor.Id));
            }

            return await query.ToListAsync(cancellationToken);
        }

        private IQueryable<Doctor> GetApprovedDoctorsQuery()
        {
            return _context.Set<Doctor>()
                .AsNoTracking()
                .Include(doctor => doctor.Specialization)
                .Where(doctor => doctor.ApprovalStatus == "Approved");
        }

        private static string EscapeLikePattern(string value)
        {
            return value.Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("%", "\\%", StringComparison.Ordinal)
                .Replace("_", "\\_", StringComparison.Ordinal)
                .Replace("[", "\\[", StringComparison.Ordinal);
        }
    }
}
