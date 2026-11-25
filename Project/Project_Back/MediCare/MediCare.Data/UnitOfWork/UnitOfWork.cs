using MediCare.Data.Data;
using MediCare.Data.Repositories;

namespace MediCare.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(
            ApplicationDbContext context,
            IDoctorRepository doctors,
            IPatientRepository patients,
            IAppointmentRepository appointments,
            ISpecializationRepository specializations)
        {
            _context = context;
            Doctors = doctors;
            Patients = patients;
            Appointments = appointments;
            Specializations = specializations;
        }

        public IDoctorRepository Doctors { get; }

        public IPatientRepository Patients { get; }

        public IAppointmentRepository Appointments { get; }

        public ISpecializationRepository Specializations { get; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
