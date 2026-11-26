using MediCare.Data.Entities;
using MediCare.Data.Repositories;

namespace MediCare.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IDoctorRepository Doctors { get; }

        IPatientRepository Patients { get; }

        IAppointmentRepository Appointments { get; }

        ISpecializationRepository Specializations { get; }

        /// <summary>Doctor working hours (used to generate free slots).</summary>
        IGenericRepository<DoctorSchedule> DoctorSchedules { get; }

        /// <summary>Doctor vacations (no slots are offered during a vacation).</summary>
        IGenericRepository<DoctorVacation> DoctorVacations { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
