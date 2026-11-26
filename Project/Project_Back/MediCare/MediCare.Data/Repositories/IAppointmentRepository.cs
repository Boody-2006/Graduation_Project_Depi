using MediCare.Data.Entities;

namespace MediCare.Data.Repositories
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        /// <summary>Appointments of a doctor on one day that still occupy a slot (Pending / Confirmed / Done).</summary>
        Task<IReadOnlyList<Appointment>> GetActiveByDoctorAndDateAsync(
            string doctorId,
            DateTime date,
            CancellationToken cancellationToken = default);

        /// <summary>One appointment with doctor, specialization and patient names included (read-only).</summary>
        Task<Appointment?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>All appointments of a patient, newest first, with details included.</summary>
        Task<IReadOnlyList<Appointment>> GetByPatientAsync(
            string patientId,
            CancellationToken cancellationToken = default);

        /// <summary>Appointments of a doctor, optionally filtered by statuses and/or one day, with details included.</summary>
        Task<IReadOnlyList<Appointment>> GetByDoctorAsync(
            string doctorId,
            IReadOnlyList<string>? statuses = null,
            DateTime? date = null,
            CancellationToken cancellationToken = default);
    }
}
