using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Configurations
{
    public class PrescriptionConfig : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.HasKey(p => p.Id);

            builder.HasOne(p => p.MedicalRecord)
                .WithOne(m => m.Prescription)
                .HasForeignKey<Prescription>(p => p.MedicalRecordId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
