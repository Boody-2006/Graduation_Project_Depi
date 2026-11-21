using MediCare.Data.Data;
using MediCare.Data.Entities;

namespace MediCare.Data.Repositories
{
    public class SpecializationRepository : GenericRepository<Specialization>, ISpecializationRepository
    {
        public SpecializationRepository(ApplicationDbContext context)
            : base(context)
        {
        }
    }
}
