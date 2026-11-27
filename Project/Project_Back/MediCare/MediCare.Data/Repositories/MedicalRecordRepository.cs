using MediCare.Data.Data;
using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Repositories
{
    public class MedicalRecordRepository : GenericRepository<MedicalRecord>, IMedicalRecordRepository
    {
        public MedicalRecordRepository(ApplicationDbContext context)
            : base(context)
        {
        }
        public async Task<IEnumerable<MedicalRecord>> GetByDoctorIdAsync(string doctorId)
        {
            return await _context.MedicalRecords
                .AsNoTracking()
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.Patient)
                        .ThenInclude(p => p.User)
                .Where(m => m.Appointment.DoctorId == doctorId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }
    }
}
