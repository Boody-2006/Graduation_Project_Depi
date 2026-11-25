using MediCare.Data.Entities;
using MediCare.Data.UnitOfWork;
using MediCare.Services.Interfaces;
using MediCare.Services.ViewModels;

namespace MediCare.Services.Implementations
{
    /// <summary>
    /// The real implementation of the doctor listing logic.
    /// It talks to the database only through IUnitOfWork -> repositories (never the DbContext).
    /// It replaces the temporary FakeDoctorService.
    /// </summary>
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;

        // The Unit of Work is injected by ASP.NET (registered by AddMediCareRepositories in MediCare.Data).
        public DoctorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc />
        public async Task<List<DoctorCardVM>> SearchAsync(DoctorFilterVM filter)
        {
            filter ??= new DoctorFilterVM();

            // The repository already returns APPROVED doctors only (business rule),
            // and applies the name / specialization / city / availability filters in SQL.
            var doctors = await _unitOfWork.Doctors.SearchAsync(
                name: filter.SearchTerm,
                specializationId: filter.SpecializationId,
                city: filter.City,
                isAvailable: filter.IsAvailable);

            // "Available" means the doctor has at least one active schedule.
            // The repository does not load schedules, so we find out who is available with a second
            // query that uses the same filters, then mark those doctors on the cards.
            HashSet<string> availableIds;

            if (filter.IsAvailable == true)
            {
                // Everything returned is already available.
                availableIds = doctors.Select(d => d.Id).ToHashSet();
            }
            else if (filter.IsAvailable == false)
            {
                // Everything returned is, by definition, NOT available.
                availableIds = new HashSet<string>();
            }
            else
            {
                var available = await _unitOfWork.Doctors.SearchAsync(
                    name: filter.SearchTerm,
                    specializationId: filter.SpecializationId,
                    city: filter.City,
                    isAvailable: true);

                availableIds = available.Select(d => d.Id).ToHashSet();
            }

            // Map the database entities to the flat objects the view needs.
            return doctors
                .Select(d => ToCard(d, availableIds.Contains(d.Id)))
                .OrderBy(card => card.FullName)
                .ToList();
        }

        /// <inheritdoc />
        public async Task<List<SpecializationOptionVM>> GetSpecializationsAsync()
        {
            var specializations = await _unitOfWork.Specializations.GetAllAsync();

            return specializations
                .Select(s => new SpecializationOptionVM { Id = s.Id, Name = s.Name ?? string.Empty })
                .OrderBy(s => s.Name)
                .ToList();
        }

        /// <summary>Converts one Doctor entity into the card shown on the page.</summary>
        private static DoctorCardVM ToCard(Doctor doctor, bool isAvailable)
        {
            var fullName = $"{doctor.User.FirstName} {doctor.User.LastName}".Trim();

            return new DoctorCardVM
            {
                DoctorId = doctor.Id,
                FullName = $"Dr. {fullName}",
                ProfileImage = doctor.User.ProfileImage,
                SpecializationId = doctor.SpecializationId,
                SpecializationName = doctor.Specialization?.Name ?? string.Empty,
                City = doctor.City,
                Address = doctor.Address,
                ConsultationFee = doctor.ConsultationFee,
                AverageRating = doctor.AverageRating,
                IsAvailable = isAvailable,
                ApprovalStatus = doctor.ApprovalStatus
            };
        }
    }
}