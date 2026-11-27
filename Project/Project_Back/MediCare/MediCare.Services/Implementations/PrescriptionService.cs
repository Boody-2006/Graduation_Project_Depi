using MediCare.Data.Entities;
using MediCare.Data.UnitOfWork;
using MediCare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Implementations
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PrescriptionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Prescription?> GetByMedicalRecordIdAsync(int medicalRecordId)
        {
            var prescriptions = await _unitOfWork.Prescriptions
                .FindAsync(p => p.MedicalRecordId == medicalRecordId);

            return prescriptions.FirstOrDefault();
        }

        public async Task<Prescription> CreateAsync(Prescription prescription)
        {
            await _unitOfWork.Prescriptions.AddAsync(prescription);

            await _unitOfWork.SaveChangesAsync();

            return prescription;
        }
    }
}
