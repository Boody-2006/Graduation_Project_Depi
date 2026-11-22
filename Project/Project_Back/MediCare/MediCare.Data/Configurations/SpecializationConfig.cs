using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations
{
    public class SpecializationConfig : IEntityTypeConfiguration<Specialization>
    {
        public void Configure(EntityTypeBuilder<Specialization> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasData(
                new Specialization
                {
                    Id = 1,
                    Name = "Neurology",
                    NameAr = "مخ وأعصاب",
                    Description = "Diagnosis and treatment of nervous system disorders.",
                    ImageUrl = "img/spec-0.jpg"
                },
                new Specialization
                {
                    Id = 2,
                    Name = "Radiology",
                    NameAr = "أشعة",
                    Description = "Medical imaging and diagnostic radiology.",
                    ImageUrl = "img/spec-1.jpg"
                },
                new Specialization
                {
                    Id = 3,
                    Name = "Neurosurgery",
                    NameAr = "جراحة مخ وأعصاب",
                    Description = "Surgical treatment of nervous system disorders.",
                    ImageUrl = "img/spec-2.jpg"
                },
                new Specialization
                {
                    Id = 4,
                    Name = "Internal Medicine",
                    NameAr = "باطنة",
                    Description = "Diagnosis and treatment of internal diseases.",
                    ImageUrl = "img/spec-3.jpg"
                },
                new Specialization
                {
                    Id = 5,
                    Name = "Cardiology",
                    NameAr = "قلب",
                    Description = "Diagnosis and treatment of heart diseases.",
                    ImageUrl = "img/spec-4.jpg"
                },
                new Specialization
                {
                    Id = 6,
                    Name = "Pediatrics",
                    NameAr = "أطفال",
                    Description = "Medical care for children.",
                    ImageUrl = "img/spec-5.jpg"
                },
                new Specialization
                {
                    Id = 7,
                    Name = "Dermatology",
                    NameAr = "جلدية",
                    Description = "Diagnosis and treatment of skin conditions.",
                    ImageUrl = "img/spec-6.jpg"
                },
                new Specialization
                {
                    Id = 8,
                    Name = "Orthopedics",
                    NameAr = "عظام",
                    Description = "Diagnosis and treatment of bone and joint disorders.",
                    ImageUrl = "img/spec-7.jpg"
                }
            );
        }
    }
}