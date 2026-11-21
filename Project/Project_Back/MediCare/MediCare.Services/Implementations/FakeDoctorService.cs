
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
                SpecializationName = "Cardiology",
                City = "Cairo",
                ConsultationFee = 500,
                AverageRating = 4.8
            },
            new DoctorCardVM
            {
                DoctorId = "2",
                FullName = "Dr. Mona Ali",
                ProfileImage = "/img/doctor-1.jpg",
                SpecializationName = "Dermatology",
                City = "Giza",
                ConsultationFee = 400,
                AverageRating = 4.6
            },
            new DoctorCardVM
            {
                DoctorId = "3",
                FullName = "Dr. Omar Khaled",
                ProfileImage = "/img/doctor-2.jpg",
                SpecializationName = "Neurology",
                City = "Cairo",
                ConsultationFee = 600,
                AverageRating = 4.9
            },
            new DoctorCardVM
            {
                DoctorId = "4",
                FullName = "Dr. Sara Mohamed",
                ProfileImage = "/img/doctor-4.jpg",
                SpecializationName = "Pediatrics",
                City = "Alexandria",
                ConsultationFee = 350,
                AverageRating = 4.7
            },
            new DoctorCardVM
            {
                DoctorId = "5",
                FullName = "Dr. Youssef Adel",
                ProfileImage = "/img/doctor-5.jpg",
                SpecializationName = "Radiology",
                City = "Cairo",
                ConsultationFee = 450,
                AverageRating = 4.5
            },
            new DoctorCardVM
            {
                DoctorId = "6",
                FullName = "Dr. Nour Samir",
                ProfileImage = "/img/doctor-6.jpg",
                SpecializationName = "Orthopedics",
                City = "Giza",
                ConsultationFee = 550,
                AverageRating = 4.8
            },
            new DoctorCardVM
            {
                DoctorId = "7",
                FullName = "Dr. Mariam Tarek",
                ProfileImage = "/img/doctor-7.jpg",
                SpecializationName = "Internal Medicine",
                City = "Cairo",
                ConsultationFee = 400,
                AverageRating = 4.4
            }
        };

        public Task<List<DoctorCardVM>> SearchAsync(DoctorFilterVM filter)
        {
            var result = _doctors.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                result = result.Where(d =>
                    d.FullName.Contains(filter.SearchTerm,
                    StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                result = result.Where(d =>
                    d.City.Equals(filter.City,
                    StringComparison.OrdinalIgnoreCase));
            }

            if (filter.SpecializationId.HasValue)
            {
                string[] specializations =
                {
                    "Neurology", "Radiology", "Neurosurgery",
                    "Internal Medicine", "Cardiology", "Pediatrics",
                    "Dermatology", "Orthopedics"
                };

                int index = filter.SpecializationId.Value - 1;

                if (index >= 0 && index < specializations.Length)
                {
                    string selected = specializations[index];

                    result = result.Where(d =>
                        d.SpecializationName == selected);
                }
            }

            return Task.FromResult(result.ToList());
        }
    }
}
