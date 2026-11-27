using MediCare.Data.Repositories;

namespace MediCare.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IDoctorRepository Doctors { get; }

        IPatientRepository Patients { get; }

        IAppointmentRepository Appointments { get; }

        ISpecializationRepository Specializations { get; }

        IMedicalRecordRepository MedicalRecords { get; }

        IPrescriptionRepository Prescriptions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
