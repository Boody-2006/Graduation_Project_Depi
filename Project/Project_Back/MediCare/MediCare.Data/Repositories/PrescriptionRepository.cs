using MediCare.Data.Data;
using MediCare.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Repositories
{
    public class PrescriptionRepository : GenericRepository<Prescription>, IPrescriptionRepository
    {
        public PrescriptionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
