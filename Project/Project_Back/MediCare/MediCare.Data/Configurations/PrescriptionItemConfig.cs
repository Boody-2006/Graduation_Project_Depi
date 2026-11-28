using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Configurations
{
    public class PrescriptionItemConfig : IEntityTypeConfiguration<PrescriptionItem>
    {
        public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.MedicationName)
                .IsRequired();

            builder.Property(p => p.Dosage)
                .IsRequired();

            builder.Property(p => p.Frequency)
                .IsRequired();

            builder.Property(p => p.Duration)
                .IsRequired();

            builder.HasOne(p => p.Prescription)
                .WithMany(p => p.PrescriptionItems)
                .HasForeignKey(p => p.PrescriptionId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
