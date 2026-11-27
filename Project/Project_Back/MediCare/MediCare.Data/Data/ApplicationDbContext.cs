
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MediCare.Data.Entities;

namespace MediCare.Data.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }

        public DbSet<DoctorVacation> DoctorVacations { get; set; }

        public DbSet<MedicalRecord> MedicalRecords { get; set; }

        public DbSet<Specialization> Specializations { get; set; }

        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<Assistant> Assistants { get; set; }

        public DbSet<HomeAddress> HomeAddresses { get; set; }

        public DbSet<Prescription> Prescriptions { get; set; }

        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Assistant>()
                .HasOne(a => a.User)
                .WithOne(u => u.Assistant)
                .HasForeignKey<Assistant>(a => a.Id)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.HomeAddress)
                .WithMany()
                .HasForeignKey(u => u.AddressId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);
        }
    }
}
