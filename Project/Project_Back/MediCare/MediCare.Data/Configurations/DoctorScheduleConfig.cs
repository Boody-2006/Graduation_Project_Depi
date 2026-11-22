using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediCare.Data.Entities;

namespace MediCare.Data.Configurations
{
    public class DoctorScheduleConfig : IEntityTypeConfiguration<DoctorSchedule>
    {
        public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasOne(s => s.Doctor)
                .WithMany(d => d.DoctorSchedules)
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}