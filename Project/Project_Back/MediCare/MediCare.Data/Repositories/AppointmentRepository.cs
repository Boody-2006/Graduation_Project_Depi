using MediCare.Data.Data;
using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<IReadOnlyList<Appointment>> GetActiveByDoctorAndDateAsync(
            string doctorId,
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);
            var activeStatuses = AppointmentStatus.Active;

            return await _context.Set<Appointment>()
                .AsNoTracking()
                .Where(appointment =>
                    appointment.DoctorId == doctorId &&
                    appointment.AppointmentDate >= dayStart &&
                    appointment.AppointmentDate < dayEnd &&
                    activeStatuses.Contains(appointment.Status))
                .ToListAsync(cancellationToken);
        }

        public async Task<Appointment?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetWithDetailsQuery()
                .FirstOrDefaultAsync(appointment => appointment.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Appointment>> GetByPatientAsync(
            string patientId,
            CancellationToken cancellationToken = default)
        {
            return await GetWithDetailsQuery()
                .Where(appointment => appointment.PatientId == patientId)
                .OrderByDescending(appointment => appointment.AppointmentDate)
                .ThenByDescending(appointment => appointment.StartTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Appointment>> GetByDoctorAsync(
            string doctorId,
            IReadOnlyList<string>? statuses = null,
            DateTime? date = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetWithDetailsQuery()
                .Where(appointment => appointment.DoctorId == doctorId);

            if (statuses != null && statuses.Count > 0)
            {
                query = query.Where(appointment => statuses.Contains(appointment.Status));
            }

            if (date.HasValue)
            {
                var dayStart = date.Value.Date;
                var dayEnd = dayStart.AddDays(1);
                query = query.Where(appointment =>
                    appointment.AppointmentDate >= dayStart &&
                    appointment.AppointmentDate < dayEnd);
            }

            return await query
                .OrderBy(appointment => appointment.AppointmentDate)
                .ThenBy(appointment => appointment.StartTime)
                .ToListAsync(cancellationToken);
        }

        private IQueryable<Appointment> GetWithDetailsQuery()
        {
            return _context.Set<Appointment>()
                .AsNoTracking()
                .Include(appointment => appointment.Doctor).ThenInclude(doctor => doctor.User)
                .Include(appointment => appointment.Doctor).ThenInclude(doctor => doctor.Specialization)
                .Include(appointment => appointment.Patient).ThenInclude(patient => patient.User);
        }
    }
}
