using MediCare.Data.Entities;
using MediCare.Data.Repositories;
using MediCare.Data.UnitOfWork;
using MediCare.Services.Interfaces;
using MediCare.Services.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace MediCare.Services.Implementations
{
    /// <summary>
    /// Handles patient booking, FullCalendar slot generation, and appointment cancel/reschedule
    /// while enforcing ownership checks through ICurrentUserService.
    /// TODO (Person 4): Delegate slot generation and conflict detection to Person 4's AppointmentEngine once merged.
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IGenericRepository<DoctorSchedule> _scheduleRepo;
        private readonly IGenericRepository<DoctorVacation> _vacationRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly string[] BlockingStatuses = { "pending", "confirmed" };

        public BookingService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IGenericRepository<DoctorSchedule> scheduleRepo,
            IGenericRepository<DoctorVacation> vacationRepo,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _scheduleRepo = scheduleRepo;
            _vacationRepo = vacationRepo;
            _userManager = userManager;
        }

        public async Task<BookingPageViewModel> GetBookingPageAsync(string? preselectedDoctorId, CancellationToken cancellationToken = default)
        {
            var approvedDoctors = await _unitOfWork.Doctors.GetApprovedAsync(cancellationToken);

            var doctorOptions = approvedDoctors
                .Select(d => new BookingDoctorOptionViewModel
                {
                    DoctorId = d.Id,
                    FullName = d.User != null ? $"Dr. {d.User.FirstName} {d.User.LastName}".Trim() : "Doctor",
                    SpecializationName = d.Specialization?.Name ?? "General",
                    City = d.City,
                    ConsultationFee = d.ConsultationFee
                })
                .OrderBy(d => d.FullName)
                .ToList();

            var selectedId = !string.IsNullOrWhiteSpace(preselectedDoctorId) && doctorOptions.Any(d => d.DoctorId == preselectedDoctorId)
                ? preselectedDoctorId
                : doctorOptions.FirstOrDefault()?.DoctorId;

            return new BookingPageViewModel
            {
                SelectedDoctorId = selectedId,
                Doctors = doctorOptions,
                BookingForm = new BookAppointmentViewModel
                {
                    DoctorId = selectedId ?? string.Empty,
                    AppointmentDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
                }
            };
        }

        public async Task<List<CalendarSlotEventViewModel>> GetAvailableSlotsAsync(
            string doctorId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
        {
            var result = new List<CalendarSlotEventViewModel>();
            if (string.IsNullOrWhiteSpace(doctorId))
                return result;

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId, cancellationToken);
            if (doctor == null || doctor.ApprovalStatus != "Approved")
                return result;

            var schedules = (await _scheduleRepo.FindAsync(s => s.DoctorId == doctorId && s.IsActive, cancellationToken)).ToList();
            var vacations = (await _vacationRepo.FindAsync(v => v.DoctorId == doctorId, cancellationToken)).ToList();

            var startDt = startDate.ToDateTime(TimeOnly.MinValue);
            var endDt = endDate.ToDateTime(TimeOnly.MaxValue);

            var bookedAppointments = await _unitOfWork.Appointments.FindAsync(
                a => a.DoctorId == doctorId
                     && a.AppointmentDate >= startDt
                     && a.AppointmentDate <= endDt
                     && BlockingStatuses.Contains(a.Status),
                cancellationToken);

            var bookedKeys = bookedAppointments
                .Select(a => (DateOnly.FromDateTime(a.AppointmentDate), a.StartTime))
                .ToHashSet();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var nowTime = TimeOnly.FromDateTime(DateTime.Now);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date < today)
                    continue;

                // Skip days when the doctor is on vacation.
                if (vacations.Any(v => date >= v.StartDate && date <= v.EndDate))
                    continue;

                var daySchedules = schedules.Where(s => s.DayOfWeek == date.DayOfWeek).ToList();

                // Fallback clinic hours if DoctorSchedule is not yet configured for this doctor (until Person 2/4 merges).
                if (!schedules.Any() && date.DayOfWeek is >= DayOfWeek.Sunday and <= DayOfWeek.Thursday)
                {
                    daySchedules.Add(new DoctorSchedule
                    {
                        DayOfWeek = date.DayOfWeek,
                        StartTime = new TimeSpan(9, 0, 0),
                        EndTime = new TimeSpan(16, 0, 0),
                        SlotMinutes = 30,
                        IsActive = true
                    });
                }

                foreach (var sched in daySchedules)
                {
                    var slotLength = Math.Clamp(sched.SlotMinutes, 10, 120);
                    var current = TimeOnly.FromTimeSpan(sched.StartTime);
                    var end = TimeOnly.FromTimeSpan(sched.EndTime);

                    while (current.AddMinutes(slotLength) <= end)
                    {
                        if (date == today && current <= nowTime)
                        {
                            current = current.AddMinutes(slotLength);
                            continue;
                        }

                        bool isBooked = bookedKeys.Contains((date, current));
                        var slotStart = date.ToDateTime(current);
                        var slotEnd = slotStart.AddMinutes(slotLength);

                        result.Add(new CalendarSlotEventViewModel
                        {
                            Id = $"{date:yyyy-MM-dd}_{current:HH:mm}",
                            Title = isBooked ? "Booked" : $"{current:HH:mm} Available",
                            Start = slotStart.ToString("yyyy-MM-ddTHH:mm:ss"),
                            End = slotEnd.ToString("yyyy-MM-ddTHH:mm:ss"),
                            IsAvailable = !isBooked,
                            BackgroundColor = isBooked ? "#edf1f5" : "#e5eff6",
                            BorderColor = isBooked ? "#c6d0d9" : "#1d5f8a",
                            TextColor = isBooked ? "#66788a" : "#14283c"
                        });

                        current = current.AddMinutes(slotLength);
                    }
                }
            }

            return result;
        }

        public async Task<(bool Success, string Message)> BookAppointmentAsync(
            BookAppointmentViewModel model,
            CancellationToken cancellationToken = default)
        {
            var patientId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(patientId))
                return (false, "You must be logged in as a patient to book an appointment.");

            var patient = await _unitOfWork.Patients.GetByIdAsync(patientId, cancellationToken);
            if (patient == null)
                return (false, "Patient profile not found for the current user.");

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(model.DoctorId, cancellationToken);
            if (doctor == null || doctor.ApprovalStatus != "Approved")
                return (false, "Selected doctor is not available for booking.");

            var appointmentDate = model.AppointmentDate.ToDateTime(TimeOnly.MinValue);
            if (model.AppointmentDate < DateOnly.FromDateTime(DateTime.Today))
                return (false, "Cannot book an appointment in the past.");

            // Check vacation overlap
            var vacations = await _vacationRepo.FindAsync(v => v.DoctorId == model.DoctorId, cancellationToken);
            if (vacations.Any(v => model.AppointmentDate >= v.StartDate && model.AppointmentDate <= v.EndDate))
                return (false, "The doctor is on vacation on the selected date.");

            // Conflict check (no double booking)
            var existing = await _unitOfWork.Appointments.FindAsync(
                a => a.DoctorId == model.DoctorId
                     && a.AppointmentDate == appointmentDate
                     && a.StartTime == model.StartTime
                     && BlockingStatuses.Contains(a.Status),
                cancellationToken);

            if (existing.Any())
                return (false, "This slot has already been booked. Please pick another time.");

            var appointment = new Appointment
            {
                DoctorId = model.DoctorId,
                PatientId = patientId,
                AppointmentDate = appointmentDate,
                StartTime = model.StartTime,
                Type = string.IsNullOrWhiteSpace(model.Type) ? "Consultation" : model.Type.Trim(),
                Status = "pending",
                Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
                Fee = doctor.ConsultationFee,
                PaymentStatus = "Unpaid",
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return (true, $"Appointment requested for {model.AppointmentDate:dd MMM yyyy} at {model.StartTime:HH:mm}.");
        }

        public async Task<List<PatientAppointmentItemViewModel>> GetMyAppointmentsAsync(CancellationToken cancellationToken = default)
        {
            var patientId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(patientId))
                return new List<PatientAppointmentItemViewModel>();

            // Ownership filter: only return appointments belonging to the logged-in patient.
            var appointments = await _unitOfWork.Appointments.FindAsync(a => a.PatientId == patientId, cancellationToken);
            var doctors = (await _unitOfWork.Doctors.GetAllAsync(cancellationToken)).ToDictionary(d => d.Id);
            var specs = (await _unitOfWork.Specializations.GetAllAsync(cancellationToken)).ToDictionary(s => s.Id, s => s.Name);

            var doctorNames = new Dictionary<string, string>();
            foreach (var docId in appointments.Select(a => a.DoctorId).Distinct())
            {
                var user = await _userManager.FindByIdAsync(docId);
                doctorNames[docId] = user != null ? $"Dr. {user.FirstName} {user.LastName}".Trim() : "Doctor";
            }

            return appointments
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a =>
                {
                    doctors.TryGetValue(a.DoctorId, out var doc);
                    var specName = doc != null && specs.TryGetValue(doc.SpecializationId, out var sName) ? sName : "General";

                    return new PatientAppointmentItemViewModel
                    {
                        Id = a.Id,
                        DoctorId = a.DoctorId,
                        DoctorName = doctorNames.TryGetValue(a.DoctorId, out var dName) ? dName : "Doctor",
                        SpecializationName = specName,
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime,
                        Type = a.Type,
                        Status = a.Status,
                        Fee = a.Fee,
                        PaymentStatus = a.PaymentStatus,
                        Notes = a.Notes,
                        CancelReason = a.CancelReason,
                        CancelledBy = a.CancelledBy
                    };
                }).ToList();
        }

        public async Task<(bool Success, string Message)> CancelAppointmentAsync(
            int appointmentId,
            string? reason,
            CancellationToken cancellationToken = default)
        {
            var patientId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(patientId))
                return (false, "Unauthorized.");

            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId, cancellationToken);

            // Strict ownership check: changing the ID in the form/URL cannot cancel another patient's appointment.
            if (appointment == null || appointment.PatientId != patientId)
                return (false, "Appointment not found or access denied.");

            if (appointment.Status is not ("pending" or "confirmed"))
                return (false, "Only pending or confirmed appointments can be cancelled.");

            appointment.Status = "cancelled";
            appointment.CancelledBy = "Patient";
            appointment.CancelReason = string.IsNullOrWhiteSpace(reason) ? "Cancelled by patient" : reason.Trim();

            await _unitOfWork.Appointments.UpdateAsync(appointment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return (true, "Appointment cancelled successfully.");
        }

        public async Task<(bool Success, string Message)> RescheduleAppointmentAsync(
            int appointmentId,
            DateOnly newDate,
            TimeOnly newTime,
            CancellationToken cancellationToken = default)
        {
            var patientId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(patientId))
                return (false, "Unauthorized.");

            var original = await _unitOfWork.Appointments.GetByIdAsync(appointmentId, cancellationToken);

            // Strict ownership check: changing the ID in the form/URL cannot reschedule another patient's appointment.
            if (original == null || original.PatientId != patientId)
                return (false, "Appointment not found or access denied.");

            if (original.Status is not ("pending" or "confirmed"))
                return (false, "Only pending or confirmed appointments can be rescheduled.");

            if (newDate < DateOnly.FromDateTime(DateTime.Today))
                return (false, "New appointment date cannot be in the past.");

            var newDateTime = newDate.ToDateTime(TimeOnly.MinValue);

            var conflicts = await _unitOfWork.Appointments.FindAsync(
                a => a.DoctorId == original.DoctorId
                     && a.AppointmentDate == newDateTime
                     && a.StartTime == newTime
                     && a.Id != original.Id
                     && BlockingStatuses.Contains(a.Status),
                cancellationToken);

            if (conflicts.Any())
                return (false, "The selected new slot is already taken. Please pick another time.");

            original.Status = "rescheduled";
            await _unitOfWork.Appointments.UpdateAsync(original);

            var replacement = new Appointment
            {
                DoctorId = original.DoctorId,
                PatientId = patientId,
                AppointmentDate = newDateTime,
                StartTime = newTime,
                Type = original.Type,
                Status = "pending",
                Notes = original.Notes,
                Fee = original.Fee,
                PaymentStatus = original.PaymentStatus,
                RescheduledFromId = original.Id,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Appointments.AddAsync(replacement, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return (true, $"Appointment rescheduled to {newDate:dd MMM yyyy} at {newTime:HH:mm}.");
        }
    }
}
