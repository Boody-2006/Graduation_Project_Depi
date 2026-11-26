using MediCare.Data.Entities;
using MediCare.Data.UnitOfWork;
using MediCare.Services.Factories;
using MediCare.Services.Interfaces;
using MediCare.Services.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private const string ApprovedStatus = "Approved";

        // Used only if a doctor's schedule has no valid SlotMinutes.
        private const int DefaultSlotMinutes = 30;

        /// <summary>The status flow: from which status we may move to which statuses.</summary>
        private static readonly Dictionary<string, string[]> AllowedTransitions = new()
        {
            [AppointmentStatus.Pending] = new[]
            {
                AppointmentStatus.Confirmed,
                AppointmentStatus.Rejected,
                AppointmentStatus.Cancelled,
                AppointmentStatus.Rescheduled
            },
            [AppointmentStatus.Confirmed] = new[]
            {
                AppointmentStatus.Done,
                AppointmentStatus.Cancelled,
                AppointmentStatus.Rescheduled
            }
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppointmentFactory _appointmentFactory;

        public AppointmentService(IUnitOfWork unitOfWork, IAppointmentFactory appointmentFactory)
        {
            _unitOfWork = unitOfWork;
            _appointmentFactory = appointmentFactory;
        }

        // =====================================================
        // Free slots
        // =====================================================

        public async Task<List<SlotVM>> GetAvailableSlotsAsync(string doctorId, DateTime date)
        {
            var day = date.Date;
            var result = new List<SlotVM>();

            if (day < DateTime.Today)
            {
                return result;
            }

            // Only approved doctors can be booked.
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
            if (doctor == null || doctor.ApprovalStatus != ApprovedStatus)
            {
                return result;
            }

            // No slots while the doctor is on vacation.
            if (await IsOnVacationAsync(doctorId, day))
            {
                return result;
            }

            // Slots come from the doctor's working hours for that weekday.
            var schedules = await GetActiveSchedulesAsync(doctorId, day);
            var allSlots = BuildSlots(schedules);
            if (allSlots.Count == 0)
            {
                return result;
            }

            var bookedRanges = await GetBookedRangesAsync(doctorId, day, schedules, null);
            var timeNow = TimeOnly.FromDateTime(DateTime.Now);

            foreach (var slot in allSlots)
            {
                // Skip slots that already started today.
                if (day == DateTime.Today && slot.Start <= timeNow)
                {
                    continue;
                }

                // Skip slots that overlap an existing appointment.
                if (bookedRanges.Any(booked => Overlaps(slot, booked)))
                {
                    continue;
                }

                result.Add(new SlotVM
                {
                    Start = day + slot.Start.ToTimeSpan(),
                    End = day + slot.End.ToTimeSpan()
                });
            }

            return result;
        }

        // =====================================================
        // Booking
        // =====================================================

        public async Task<AppointmentResult> BookAppointmentAsync(string patientId, BookAppointmentVM model)
        {
            var day = model.AppointmentDate.Date;

            var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
            if (patient == null)
            {
                return AppointmentResult.Failed("Patient not found / المريض غير موجود.");
            }

            if (!AppointmentTypes.IsValid(model.Type))
            {
                return AppointmentResult.Failed("Invalid appointment type / نوع الحجز غير صحيح.");
            }

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(model.DoctorId);
            if (doctor == null || doctor.ApprovalStatus != ApprovedStatus)
            {
                return AppointmentResult.Failed("Doctor not found or not approved / الطبيب غير موجود أو لم يتم اعتماده.");
            }

            // Conflict detection: past time, vacation, outside schedule, already booked.
            var slotError = await CheckSlotAsync(doctor.Id, day, model.StartTime, null);
            if (slotError != null)
            {
                return AppointmentResult.Failed(slotError);
            }

            var appointment = _appointmentFactory.Create(
                model.Type,
                doctor,
                patientId,
                day,
                model.StartTime,
                string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim());

            await _unitOfWork.Appointments.AddAsync(appointment);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Safety net: the unique index refused it because someone booked the same slot at the same moment.
                return AppointmentResult.Failed(SlotTakenMessage);
            }

            return AppointmentResult.Success(appointment.Id);
        }

        // =====================================================
        // Status flow
        // =====================================================

        public Task<AppointmentResult> AcceptAsync(int appointmentId, string doctorId)
        {
            return ChangeStatusByDoctorAsync(appointmentId, doctorId, AppointmentStatus.Confirmed, null);
        }

        public Task<AppointmentResult> RejectAsync(int appointmentId, string doctorId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return Task.FromResult(AppointmentResult.Failed(
                    "A rejection reason is required / يجب كتابة سبب الرفض."));
            }

            return ChangeStatusByDoctorAsync(appointmentId, doctorId, AppointmentStatus.Rejected, reason.Trim());
        }

        public Task<AppointmentResult> CompleteAsync(int appointmentId, string doctorId)
        {
            return ChangeStatusByDoctorAsync(appointmentId, doctorId, AppointmentStatus.Done, null);
        }

        public async Task<AppointmentResult> CancelAsync(int appointmentId, string userId, string? reason = null)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);

            // Ownership check: only the patient or the doctor of this appointment.
            if (appointment == null ||
                (appointment.PatientId != userId && appointment.DoctorId != userId))
            {
                return AppointmentResult.Failed(NotFoundMessage);
            }

            if (!CanChangeStatus(appointment.Status, AppointmentStatus.Cancelled))
            {
                return AppointmentResult.Failed("This appointment cannot be cancelled / لا يمكن إلغاء هذا الموعد.");
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancelledBy = appointment.DoctorId == userId ? "Doctor" : "Patient";
            appointment.CancelReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return AppointmentResult.Success(appointment.Id);
        }

        public async Task<AppointmentResult> RescheduleAsync(
            int appointmentId,
            string patientId,
            DateTime newDate,
            TimeOnly newStartTime)
        {
            var oldAppointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);

            // Ownership check: only the patient who owns it.
            if (oldAppointment == null || oldAppointment.PatientId != patientId)
            {
                return AppointmentResult.Failed(NotFoundMessage);
            }

            if (!CanChangeStatus(oldAppointment.Status, AppointmentStatus.Rescheduled))
            {
                return AppointmentResult.Failed("This appointment cannot be rescheduled / لا يمكن تغيير موعد هذا الحجز.");
            }

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(oldAppointment.DoctorId);
            if (doctor == null || doctor.ApprovalStatus != ApprovedStatus)
            {
                return AppointmentResult.Failed("Doctor not found or not approved / الطبيب غير موجود أو لم يتم اعتماده.");
            }

            var day = newDate.Date;

            // The old appointment is ignored, so moving inside its own time is not a conflict.
            var slotError = await CheckSlotAsync(doctor.Id, day, newStartTime, oldAppointment.Id);
            if (slotError != null)
            {
                return AppointmentResult.Failed(slotError);
            }

            // New appointment keeps a link to the old one (RescheduledFromId).
            var newAppointment = _appointmentFactory.Create(
                oldAppointment.Type,
                doctor,
                patientId,
                day,
                newStartTime,
                oldAppointment.Notes,
                oldAppointment.Id);

            // Keep the fee and payment state that were already agreed.
            newAppointment.Fee = oldAppointment.Fee;
            newAppointment.PaymentStatus = oldAppointment.PaymentStatus;

            oldAppointment.Status = AppointmentStatus.Rescheduled;

            await _unitOfWork.Appointments.AddAsync(newAppointment);

            try
            {
                // One SaveChanges: either both changes are saved or none.
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return AppointmentResult.Failed(SlotTakenMessage);
            }

            return AppointmentResult.Success(newAppointment.Id);
        }

        // =====================================================
        // Queries (for dashboards and "My appointments")
        // =====================================================

        public async Task<AppointmentVM?> GetByIdAsync(int appointmentId, string userId)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdWithDetailsAsync(appointmentId);

            if (appointment == null ||
                (appointment.PatientId != userId && appointment.DoctorId != userId))
            {
                return null;
            }

            return ToViewModel(appointment);
        }

        public async Task<List<AppointmentVM>> GetPatientAppointmentsAsync(string patientId)
        {
            var appointments = await _unitOfWork.Appointments.GetByPatientAsync(patientId);
            return appointments.Select(ToViewModel).ToList();
        }

        public async Task<List<AppointmentVM>> GetTodayAppointmentsAsync(string doctorId)
        {
            var appointments = await _unitOfWork.Appointments.GetByDoctorAsync(
                doctorId,
                new[] { AppointmentStatus.Confirmed, AppointmentStatus.Done },
                DateTime.Today);

            return appointments.Select(ToViewModel).ToList();
        }

        public async Task<List<AppointmentVM>> GetPendingRequestsAsync(string doctorId)
        {
            var appointments = await _unitOfWork.Appointments.GetByDoctorAsync(
                doctorId,
                new[] { AppointmentStatus.Pending });

            return appointments.Select(ToViewModel).ToList();
        }

        // =====================================================
        // Private helpers
        // =====================================================

        private const string NotFoundMessage = "Appointment not found / الموعد غير موجود.";

        private const string SlotTakenMessage = "This time slot is already booked / هذا الموعد محجوز بالفعل.";

        /// <summary>A time range inside one day (used for slots and for booked appointments).</summary>
        private readonly record struct TimeSlot(TimeOnly Start, TimeOnly End);

        /// <summary>Changes the status of an appointment on behalf of its doctor (accept / reject / done).</summary>
        private async Task<AppointmentResult> ChangeStatusByDoctorAsync(
            int appointmentId,
            string doctorId,
            string newStatus,
            string? reason)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);

            // Ownership check: a doctor can only change his own appointments.
            if (appointment == null || appointment.DoctorId != doctorId)
            {
                return AppointmentResult.Failed(NotFoundMessage);
            }

            if (!CanChangeStatus(appointment.Status, newStatus))
            {
                return AppointmentResult.Failed(
                    $"Cannot change status from {appointment.Status} to {newStatus} / لا يمكن تغيير الحالة من {appointment.Status} إلى {newStatus}.");
            }

            appointment.Status = newStatus;

            if (newStatus == AppointmentStatus.Rejected)
            {
                // The rejection reason is stored in CancelReason.
                appointment.CancelReason = reason;
                appointment.CancelledBy = "Doctor";
            }

            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return AppointmentResult.Success(appointment.Id);
        }

        private static bool CanChangeStatus(string from, string to)
        {
            return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
        }

        /// <summary>
        /// Conflict detection. Returns an error message if the slot cannot be booked, or null if it is free.
        /// ignoreAppointmentId is used when rescheduling, so the old appointment does not block the new slot.
        /// </summary>
        private async Task<string?> CheckSlotAsync(
            string doctorId,
            DateTime day,
            TimeOnly startTime,
            int? ignoreAppointmentId)
        {
            if (day < DateTime.Today ||
                (day == DateTime.Today && startTime <= TimeOnly.FromDateTime(DateTime.Now)))
            {
                return "You cannot book a time in the past / لا يمكن الحجز في وقت سابق.";
            }

            if (await IsOnVacationAsync(doctorId, day))
            {
                return "The doctor is on vacation on this date / الطبيب في إجازة في هذا التاريخ.";
            }

            var schedules = await GetActiveSchedulesAsync(doctorId, day);
            var slots = BuildSlots(schedules);

            // The requested time must be exactly one of the doctor's slots.
            var index = slots.FindIndex(slot => slot.Start == startTime);
            if (index < 0)
            {
                return "This time is outside the doctor's working hours / هذا الوقت خارج مواعيد عمل الطبيب.";
            }

            var bookedRanges = await GetBookedRangesAsync(doctorId, day, schedules, ignoreAppointmentId);
            if (bookedRanges.Any(booked => Overlaps(slots[index], booked)))
            {
                return SlotTakenMessage;
            }

            return null;
        }

        private async Task<bool> IsOnVacationAsync(string doctorId, DateTime day)
        {
            var date = DateOnly.FromDateTime(day);

            var vacations = await _unitOfWork.DoctorVacations.FindAsync(vacation =>
                vacation.DoctorId == doctorId &&
                vacation.StartDate <= date &&
                vacation.EndDate >= date);

            return vacations.Count > 0;
        }

        private async Task<IReadOnlyList<DoctorSchedule>> GetActiveSchedulesAsync(string doctorId, DateTime day)
        {
            var weekDay = day.DayOfWeek;

            return await _unitOfWork.DoctorSchedules.FindAsync(schedule =>
                schedule.DoctorId == doctorId &&
                schedule.IsActive &&
                schedule.DayOfWeek == weekDay);
        }

        /// <summary>Cuts each working period into slots of SlotMinutes.</summary>
        private static List<TimeSlot> BuildSlots(IReadOnlyList<DoctorSchedule> schedules)
        {
            var slots = new List<TimeSlot>();

            foreach (var schedule in schedules.OrderBy(s => s.StartTime))
            {
                if (schedule.SlotMinutes <= 0)
                {
                    continue;
                }

                var step = TimeSpan.FromMinutes(schedule.SlotMinutes);
                var current = schedule.StartTime;

                while (current + step <= schedule.EndTime)
                {
                    slots.Add(new TimeSlot(
                        TimeOnly.FromTimeSpan(current),
                        TimeOnly.FromTimeSpan(current + step)));

                    current += step;
                }
            }

            return slots.DistinctBy(slot => slot.Start).ToList();
        }

        /// <summary>
        /// The time ranges already taken on that day.
        /// The Appointment table has no end time, so the end is start + the SlotMinutes of the matching schedule.
        /// </summary>
        private async Task<List<TimeSlot>> GetBookedRangesAsync(
            string doctorId,
            DateTime day,
            IReadOnlyList<DoctorSchedule> schedules,
            int? ignoreAppointmentId)
        {
            var appointments = await _unitOfWork.Appointments.GetActiveByDoctorAndDateAsync(doctorId, day);
            var ranges = new List<TimeSlot>();

            foreach (var appointment in appointments)
            {
                if (ignoreAppointmentId.HasValue && appointment.Id == ignoreAppointmentId.Value)
                {
                    continue;
                }

                var minutes = GetSlotMinutes(appointment.StartTime, schedules);
                ranges.Add(new TimeSlot(appointment.StartTime, appointment.StartTime.AddMinutes(minutes)));
            }

            return ranges;
        }

        private static int GetSlotMinutes(TimeOnly start, IReadOnlyList<DoctorSchedule> schedules)
        {
            var match = schedules.FirstOrDefault(schedule =>
                start >= TimeOnly.FromTimeSpan(schedule.StartTime) &&
                start < TimeOnly.FromTimeSpan(schedule.EndTime))
                ?? schedules.FirstOrDefault();

            return match != null && match.SlotMinutes > 0 ? match.SlotMinutes : DefaultSlotMinutes;
        }

        /// <summary>Two ranges overlap if each one starts before the other ends.</summary>
        private static bool Overlaps(TimeSlot a, TimeSlot b)
        {
            return a.Start < b.End && b.Start < a.End;
        }

        private static AppointmentVM ToViewModel(Appointment appointment)
        {
            return new AppointmentVM
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                DoctorName = $"Dr. {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}".Trim(),
                SpecializationName = appointment.Doctor.Specialization.Name,
                PatientId = appointment.PatientId,
                PatientName = $"{appointment.Patient.User.FirstName} {appointment.Patient.User.LastName}".Trim(),
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                Type = appointment.Type,
                Status = appointment.Status,
                Notes = appointment.Notes,
                CancelReason = appointment.CancelReason,
                CancelledBy = appointment.CancelledBy,
                Fee = appointment.Fee,
                PaymentStatus = appointment.PaymentStatus,
                RescheduledFromId = appointment.RescheduledFromId
            };
        }
    }
}
