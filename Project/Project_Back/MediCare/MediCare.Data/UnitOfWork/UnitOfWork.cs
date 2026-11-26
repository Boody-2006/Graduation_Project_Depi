using MediCare.Data.Data;
using MediCare.Data.Entities;
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
            ISpecializationRepository specializations,
            IGenericRepository<DoctorSchedule> doctorSchedules,
            IGenericRepository<DoctorVacation> doctorVacations)
        {
            _context = context;
            Doctors = doctors;
            Patients = patients;
            Appointments = appointments;
            Specializations = specializations;
            DoctorSchedules = doctorSchedules;
            DoctorVacations = doctorVacations;
        }

        public IDoctorRepository Doctors { get; }

        public IPatientRepository Patients { get; }

        public IAppointmentRepository Appointments { get; }

        public ISpecializationRepository Specializations { get; }

        public IGenericRepository<DoctorSchedule> DoctorSchedules { get; }

        public IGenericRepository<DoctorVacation> DoctorVacations { get; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
