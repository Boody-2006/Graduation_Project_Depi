using System.Security.Claims;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MediCare.Services.Implementations
{
    /// <summary>
    /// Reads the current logged-in user's ID, email, and roles from HttpContext.
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string? Email => User?.FindFirstValue(ClaimTypes.Email);

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

        public bool IsInRole(string role) => User?.IsInRole(role) == true;
    }
}
