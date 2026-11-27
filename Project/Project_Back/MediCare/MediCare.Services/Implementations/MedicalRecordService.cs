using MediCare.Data.Entities;
using MediCare.Data.UnitOfWork;
using MediCare.Services.Interfaces;
using MediCare.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Implementations
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicalRecordService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<MedicalRecord>> GetAllAsync()
        {
            return await _unitOfWork.MedicalRecords.GetAllAsync();
        }

        public async Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId)
        {
            var records = await _unitOfWork.MedicalRecords.FindAsync(m => m.AppointmentId == appointmentId);

            return records.FirstOrDefault();
        }

        public async Task<MedicalRecord> CreateAsync(MedicalRecord medicalRecord)
        {
            await _unitOfWork.MedicalRecords.AddAsync(medicalRecord);

            await _unitOfWork.SaveChangesAsync();

            return medicalRecord;
        }
        public async Task<MedicalRecordViewModel> PrepareCreateAsync(
     int appointmentId)
        {
            var appointment = await _unitOfWork.Appointments
                .GetByIdAsync(appointmentId);

            if (appointment == null)
            {
                throw new InvalidOperationException(
                    "Appointment not found.");
            }

            var existingRecord =
                await GetByAppointmentIdAsync(appointmentId);

            if (existingRecord != null)
            {
                throw new InvalidOperationException(
                    "A medical record already exists for this appointment.");
            }

            if (appointment.Status != "Done")
            {
                throw new InvalidOperationException(
                    "This appointment is not completed yet.");
            }

            return new MedicalRecordViewModel
            {
                AppointmentId = appointmentId,
                Medicines = new List<PrescriptionItemViewModel>
        {
            new PrescriptionItemViewModel()
        }
            };
        }
        public async Task<MedicalRecord> CreateWithPrescriptionAsync(MedicalRecordViewModel model)
        {
            var appointment = await _unitOfWork.Appointments
                .GetByIdAsync(model.AppointmentId);

            if (appointment == null)
            {
                throw new InvalidOperationException(
                    "Appointment not found.");
            }

            if (appointment.Status != "Done")
            {
                throw new InvalidOperationException(
                    "Medical records can only be created for completed appointments.");
            }

            var existingRecord =
                await GetByAppointmentIdAsync(model.AppointmentId);

            if (existingRecord != null)
            {
                throw new InvalidOperationException(
                    "A medical record already exists for this appointment.");
            }

            if (model.Medicines == null || !model.Medicines.Any())
            {
                throw new InvalidOperationException(
                    "At least one medicine is required.");
            }

            var medicalRecord = new MedicalRecord
            {
                AppointmentId = model.AppointmentId,
                Symptoms = model.Symptoms,
                Diagnosis = model.Diagnosis,
                TreatmentNotes = model.TreatmentNotes,
                CreatedAt = DateTime.UtcNow,

                Prescription = new Prescription
                {
                    CreatedAt = DateTime.UtcNow,

                    PrescriptionItems = model.Medicines.Select(m =>
                        new PrescriptionItem
                        {
                            MedicationName = m.MedicationName,
                            Dosage = m.Dosage,
                            Frequency = m.Frequency,
                            Duration = m.Duration,
                            Instructions = m.Instructions
                        }).ToList()
                }
            };

            await _unitOfWork.MedicalRecords.AddAsync(medicalRecord);

            await _unitOfWork.SaveChangesAsync();

            return medicalRecord;
        }
        public async Task<IEnumerable<MedicalRecordIndexViewModel>>GetDoctorMedicalRecordsAsync()
        {
            // هنضيف ICurrentUserService هنا بعد دمج Person 5

            throw new NotImplementedException();
        }
    }
}
