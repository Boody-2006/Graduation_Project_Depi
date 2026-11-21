
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

        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<Assistant> Assistants { get; set; }

        public DbSet<HomeAddress> HomeAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.Id);

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.Id);

            modelBuilder.Entity<Assistant>()
                .HasOne(a => a.User)
                .WithOne(u => u.Assistant)
                .HasForeignKey<Assistant>(a => a.Id);


            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.HomeAddress)
                .WithMany()
                .HasForeignKey(u => u.AddressId);

        }
    }
}
