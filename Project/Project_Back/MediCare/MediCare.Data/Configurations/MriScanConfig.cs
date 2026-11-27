using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Configurations
{
    public class MriScanConfig : IEntityTypeConfiguration<MriScan>
    {
        public void Configure(EntityTypeBuilder<MriScan> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.FileUrl)
                .IsRequired();

            builder.HasOne(m => m.Patient)
                .WithMany(p => p.MriScans)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}