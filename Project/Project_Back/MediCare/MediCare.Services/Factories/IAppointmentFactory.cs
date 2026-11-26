using MediCare.Data.Entities;

namespace MediCare.Services.Factories
{
    public interface IAppointmentFactory
    {
        /// <summary>Builds a new Pending appointment according to its type (type decides the fee).</summary>
        Appointment Create(
            string type,
            Doctor doctor,
            string patientId,
            DateTime date,
            TimeOnly startTime,
            string? notes = null,
            int? rescheduledFromId = null);
    }
}
