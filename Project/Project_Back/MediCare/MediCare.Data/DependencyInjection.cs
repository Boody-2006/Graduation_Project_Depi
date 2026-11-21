using MediCare.Data.Repositories;
using MediCare.Data.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace MediCare.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMediCareRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<ISpecializationRepository, SpecializationRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
