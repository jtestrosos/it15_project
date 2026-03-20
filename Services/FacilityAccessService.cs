using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using production_system.Data;

namespace production_system.Services
{
    public class FacilityAccessService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IServiceScopeFactory _scopeFactory;

        public FacilityAccessService(
            AuthenticationStateProvider authStateProvider,
            IServiceScopeFactory scopeFactory)
        {
            _authStateProvider = authStateProvider;
            _scopeFactory = scopeFactory;
        }

        public async Task<int?> GetCurrentUserFacilityIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var userId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return null;

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            return user?.FacilityID;
        }

        public async Task<bool> IsSuperAdminAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.IsInRole("Superadmin");
        }

        public async Task<string?> GetCurrentUserIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}

