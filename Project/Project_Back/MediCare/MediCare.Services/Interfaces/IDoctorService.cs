using MediCare.Services.ViewModels;

namespace MediCare.Services.Interfaces
{
    /// <summary>
    /// Business logic for the public doctor listing page.
    /// Controllers talk to this interface and never touch the database directly.
    /// </summary>
    public interface IDoctorService
    {
        /// <summary>
        /// Returns the APPROVED doctors that match the filter (name, specialization, city, availability).
        /// Every property of the filter is optional: an empty value means "do not filter by this".
        /// </summary>
        Task<List<DoctorCardVM>> SearchAsync(DoctorFilterVM filter);

        /// <summary>Returns all specializations, used to fill the specialization dropdown.</summary>
        Task<List<SpecializationOptionVM>> GetSpecializationsAsync();
    }
}