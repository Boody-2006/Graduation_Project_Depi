using MediCare.Data.Repositories;
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

            // Fully qualified: "UnitOfWork" is both a namespace and a class name.
            services.AddScoped<MediCare.Data.UnitOfWork.IUnitOfWork, MediCare.Data.UnitOfWork.UnitOfWork>();

            return services;
        }
    }
}
