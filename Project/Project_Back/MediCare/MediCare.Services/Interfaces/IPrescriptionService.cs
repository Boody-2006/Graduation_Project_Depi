using MediCare.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Interfaces
{
    public interface IPrescriptionService
    {
        Task<Prescription?> GetByMedicalRecordIdAsync(int medicalRecordId);

        Task<Prescription> CreateAsync(Prescription prescription);
    }
}
