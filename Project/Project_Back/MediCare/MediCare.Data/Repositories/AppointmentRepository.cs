using MediCare.Data.Data;
using MediCare.Data.Entities;

namespace MediCare.Data.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(ApplicationDbContext context)
            : base(context)
        {
        }
    }
}
