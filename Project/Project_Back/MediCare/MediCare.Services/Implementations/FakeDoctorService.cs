using MediCare.Services.Interfaces;
using MediCare.Services.ViewModels;

namespace MediCare.Services.Implementations
{
    public class FakeDoctorService : IDoctorService
    {
        private readonly List<DoctorCardVM> _doctors = new()
        {
            new DoctorCardVM
            {
                DoctorId = "1",
                FullName = "Dr. Ahmed Hassan",
                ProfileImage = "/img/doctor-0.jpg",
                SpecializationId = 5,
                SpecializationName = "Cardiology",
                City = "Cairo",
                ConsultationFee = 500,
                AverageRating = 4.8,
                IsAvailable = true,
                ApprovalStatus = "Approved"
            },
            new DoctorCardVM
            {
                DoctorId = "2",
                FullName = "Dr. Mona Ali",
                ProfileImage = "/img/doctor-1.jpg",
                SpecializationId = 7,
                SpecializationName = "Dermatology",
                City = "Giza",
                ConsultationFee = 400,
                AverageRating = 4.6,
                IsAvailable = false,
                ApprovalStatus = "Approved"
            },
            new DoctorCardVM
            {
                DoctorId = "3",
                FullName = "Dr. Omar Khaled",
                ProfileImage = "/img/doctor-2.jpg",
                SpecializationId = 1,
                SpecializationName = "Neurology",
                City = "Cairo",
                ConsultationFee = 600,
                AverageRating = 4.9,
                IsAvailable = true,
                ApprovalStatus = "Approved"
            },
            new DoctorCardVM
            {
                DoctorId = "4",
                FullName = "Dr. Sara Mohamed",
                ProfileImage = "/img/doctor-4.jpg",
                SpecializationId = 6,
                SpecializationName = "Pediatrics",
                City = "Alexandria",
                ConsultationFee = 350,
                AverageRating = 4.7,
                IsAvailable = true,
                ApprovalStatus = "Approved"
            },
            new DoctorCardVM
            {
                DoctorId = "5",
                FullName = "Dr. Youssef Adel",
                ProfileImage = "/img/doctor-5.jpg",
                SpecializationId = 2,
                SpecializationName = "Radiology",
                City = "Cairo",
                ConsultationFee = 450,
                AverageRating = 4.5,
                IsAvailable = false,
                ApprovalStatus = "Approved"
            },
            new DoctorCardVM
            {
                DoctorId = "6",
                FullName = "Dr. Nour Samir",
                ProfileImage = "/img/doctor-6.jpg",
                SpecializationId = 8,
                SpecializationName = "Orthopedics",
                City = "Giza",
                ConsultationFee = 550,
                AverageRating = 4.8,
                IsAvailable = true,
                ApprovalStatus = "Approved"
            },
            new DoctorCardVM
            {
                DoctorId = "7",
                FullName = "Dr. Mariam Tarek",
                ProfileImage = "/img/doctor-7.jpg",
                SpecializationId = 4,
                SpecializationName = "Internal Medicine",
                City = "Cairo",
                ConsultationFee = 400,
                AverageRating = 4.4,
                IsAvailable = true,
                ApprovalStatus = "Approved"
            }
        };

        public Task<List<DoctorCardVM>> SearchAsync(DoctorFilterVM filter)
        {
            var result = _doctors.AsEnumerable();

            // Only show approved doctors
            result = result.Where(d => d.ApprovalStatus == "Approved");

            // Search by name
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                result = result.Where(d =>
                    d.FullName.Contains(
                        filter.SearchTerm,
                        StringComparison.OrdinalIgnoreCase));
            }

            // Filter by city
            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                result = result.Where(d =>
                    d.City.Equals(
                        filter.City,
                        StringComparison.OrdinalIgnoreCase));
            }

            // Filter by specialization
            if (filter.SpecializationId.HasValue)
            {
                result = result.Where(d =>
                    d.SpecializationId == filter.SpecializationId.Value);
            }

            // Filter by availability
            if (filter.IsAvailable.HasValue)
            {
                result = result.Where(d =>
                    d.IsAvailable == filter.IsAvailable.Value);
            }

            return Task.FromResult(result.ToList());
        }
    }
}