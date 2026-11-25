using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediCare.Data.Entities;

namespace MediCare.Data.Configurations
{
    public class DoctorVacationConfig : IEntityTypeConfiguration<DoctorVacation>
    {
        public void Configure(EntityTypeBuilder<DoctorVacation> builder)
        {
            builder.HasKey(v => v.Id);

            builder.HasOne(v => v.Doctor)
                .WithMany(d => d.DoctorVacations)
                .HasForeignKey(v => v.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}