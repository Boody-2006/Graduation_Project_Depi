namespace MediCare.Services.Interfaces
{
    /// <summary>
    /// Provides the currently authenticated user's identity and role claims
    /// so services can enforce ownership checks without touching HttpContext directly.
    /// </summary>
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
    }
}
