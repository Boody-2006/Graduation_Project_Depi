using MediCare.Data.Entities;

namespace MediCare.Services.Factories
{
    /// <summary>
    /// Factory pattern: one place that knows how to build an Appointment for each type.
    /// Consultation = full doctor fee, FollowUp = half of the doctor fee.
    /// To add a new type, add a constant in AppointmentTypes and a case below.
    /// </summary>
    public class AppointmentFactory : IAppointmentFactory
    {
        public Appointment Create(
            string type,
            Doctor doctor,
            string patientId,
            DateTime date,
            TimeOnly startTime,
            string? notes = null,
            int? rescheduledFromId = null)
        {
            decimal fee;

            switch (type)
            {
                case AppointmentTypes.Consultation:
                    fee = doctor.ConsultationFee;
                    break;
                case AppointmentTypes.FollowUp:
                    fee = Math.Round(doctor.ConsultationFee / 2, 2);
                    break;
                default:
                    throw new ArgumentException($"Unknown appointment type: {type}", nameof(type));
            }

            return new Appointment
            {
                DoctorId = doctor.Id,
                PatientId = patientId,
                AppointmentDate = date.Date,
                StartTime = startTime,
                Type = type,
                Status = AppointmentStatus.Pending,
                PaymentStatus = "Unpaid",
                Fee = fee,
                Notes = notes,
                RescheduledFromId = rescheduledFromId
            };
        }
    }
}
