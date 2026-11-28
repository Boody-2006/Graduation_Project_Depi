using MediCare.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Repositories
{
    public interface IMedicalRecordRepository : IGenericRepository<MedicalRecord>
    {
        Task<IEnumerable<MedicalRecord>> GetByDoctorIdAsync(string doctorId);
    }
}
