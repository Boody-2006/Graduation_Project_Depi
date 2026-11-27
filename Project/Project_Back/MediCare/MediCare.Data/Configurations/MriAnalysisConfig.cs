using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Configurations
{
    public class MriAnalysisConfig : IEntityTypeConfiguration<MriAnalysis>  
    {
        public void Configure(EntityTypeBuilder<MriAnalysis> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.AnalysisResult)
                .IsRequired();

            builder.HasOne(a => a.MriScan)
                .WithMany()
                .HasForeignKey(a => a.MriScanId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
