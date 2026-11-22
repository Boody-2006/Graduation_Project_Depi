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
            string? name = null,
            int? specializationId = null,
            string? city = null,
            bool? isAvailable = null,
            DayOfWeek? availableDay = null,
            TimeSpan? availableAt = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetApprovedDoctorsQuery();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var namePattern = $"%{EscapeLikePattern(name.Trim())}%";
                query = query.Where(doctor => EF.Functions.Like(
                    doctor.User.FirstName + " " + doctor.User.LastName, namePattern, "\\"));
            }

            if (specializationId.HasValue)
            {
                query = query.Where(doctor => doctor.SpecializationId == specializationId.Value);
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                var cityPattern = $"%{EscapeLikePattern(city.Trim())}%";
                query = query.Where(doctor => EF.Functions.Like(doctor.City, cityPattern, "\\"));
            }

            // Available = the doctor has at least one active schedule.
            if (isAvailable.HasValue)
            {
                var scheduledDoctorIds = _context.Set<DoctorSchedule>()
                    .Where(schedule => schedule.IsActive)
                    .Select(schedule => schedule.DoctorId);

                query = isAvailable.Value
                    ? query.Where(doctor => scheduledDoctorIds.Contains(doctor.Id))
                    : query.Where(doctor => !scheduledDoctorIds.Contains(doctor.Id));
            }

            // Specific day (time is optional).
            if (availableDay.HasValue)
            {
                var day = availableDay.Value;

                var schedules = _context.Set<DoctorSchedule>()
                    .Where(schedule => schedule.IsActive && schedule.DayOfWeek == day);

                if (availableAt.HasValue)
                {
                    var time = availableAt.Value;
                    schedules = schedules.Where(schedule =>
                        schedule.StartTime <= time && schedule.EndTime > time);
                }

                var availableDoctorIds = schedules.Select(schedule => schedule.DoctorId);
                query = query.Where(doctor => availableDoctorIds.Contains(doctor.Id));
            }

            return await query.ToListAsync(cancellationToken);
        }

        private IQueryable<Doctor> GetApprovedDoctorsQuery()
        {
            return _context.Set<Doctor>()
                .AsNoTracking()
                .Include(doctor => doctor.Specialization)
                .Include(doctor => doctor.User)
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
