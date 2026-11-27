using MediCare.Data.Entities;
using MediCare.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Interfaces
{
    public interface IMedicalRecordService
    {
        Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId);

        Task<MedicalRecord> CreateAsync(MedicalRecord medicalRecord);

        Task<MedicalRecordViewModel> PrepareCreateAsync(int appointmentId);

        Task<MedicalRecord> CreateWithPrescriptionAsync(MedicalRecordViewModel model);

        Task<IReadOnlyList<MedicalRecord>> GetAllAsync();

        Task<IEnumerable<MedicalRecordIndexViewModel>>GetDoctorMedicalRecordsAsync();
    }
}
