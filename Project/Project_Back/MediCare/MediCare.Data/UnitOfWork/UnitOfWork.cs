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
            IMedicalRecordRepository medicalRecords,
            IPrescriptionRepository prescriptions)
        {
            _context = context;
            Doctors = doctors;
            Patients = patients;
            Appointments = appointments;
            Specializations = specializations;
            MedicalRecords = medicalRecords;
            Prescriptions = prescriptions;
        }

        public IDoctorRepository Doctors { get; }

        public IPatientRepository Patients { get; }

        public IAppointmentRepository Appointments { get; }

        public ISpecializationRepository Specializations { get; }

        public IMedicalRecordRepository MedicalRecords { get; }

        public IPrescriptionRepository Prescriptions { get; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
