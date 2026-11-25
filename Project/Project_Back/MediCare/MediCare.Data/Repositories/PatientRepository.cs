using MediCare.Data.Data;
using MediCare.Data.Entities;

namespace MediCare.Data.Repositories
{
    public class PatientRepository : GenericRepository<Patient>, IPatientRepository
    {
        public PatientRepository(ApplicationDbContext context)
            : base(context)
        {
        }
    }
}
