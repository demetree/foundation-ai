//
// VolunteerHubService.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Business logic for the Volunteer Self-Service Hub.
// Provides session resolution, profile retrieval, and helper methods.
//
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Foundation.Security.Database;
using Foundation.Scheduler.Database;

namespace Foundation.Scheduler.Services
{
    public class VolunteerHubService
    {
        private readonly SchedulerContext _schedulerDb;

        public VolunteerHubService(SchedulerContext schedulerDb)
        {
            _schedulerDb = schedulerDb;
        }

        /// <summary>
        /// Result of resolving a volunteer hub session from the request header.
        /// </summary>
        public record SessionResolution(SecurityUser? User, bool IsValid, string? ErrorMessage);

        /// <summary>
        /// Session data containing the resolved volunteer information.
        /// </summary>
        public record VolunteerSession(
            int SecurityUserId,
            Guid UserGuid,
            int ResourceId,
            int ProfileId,
            Guid TenantGuid);

        /// <summary>
        /// Resolves the session token and returns the SecurityUser with validation status.
        /// </summary>
        public async Task<SessionResolution> ResolveSessionUserAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new SessionResolution(null, false, "Session token required.");
            }

            using (SecurityContext securityDb = new SecurityContext())
            {
                SecurityUser? user = await securityDb.SecurityUsers
                    .Where(u => u.authenticationToken == token)
                    .Where(u => u.active == true && u.deleted == false)
                    .FirstOrDefaultAsync();

                if (user == null || user.authenticationTokenExpiry == null || user.authenticationTokenExpiry < DateTime.UtcNow)
                {
                    return new SessionResolution(null, false, "Session expired or invalid.");
                }

                return new SessionResolution(user, true, null);
            }
        }

        /// <summary>
        /// Resolves the session token and returns full volunteer session data.
        /// </summary>
        public async Task<VolunteerSession?> ResolveSessionAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            using (SecurityContext securityDb = new SecurityContext())
            {
                var secUser = await securityDb.SecurityUsers
                    .FirstOrDefaultAsync(u => u.authenticationToken == token &&
                                              u.authenticationTokenExpiry > DateTime.UtcNow);

                if (secUser == null) return null;

                // Resolve volunteer profile
                var profile = await _schedulerDb.VolunteerProfiles
                    .FirstOrDefaultAsync(vp => vp.linkedUserGuid == secUser.objectGuid && vp.active && !vp.deleted);

                if (profile == null) return null;

                // Get tenant from a tenant user mapping
                var tenantUser = await securityDb.SecurityTenantUsers
                    .FirstOrDefaultAsync(tu => tu.securityUserId == secUser.id);

                return new VolunteerSession(
                    secUser.id,
                    secUser.objectGuid,
                    profile.resourceId,
                    profile.id,
                    tenantUser?.securityTenant?.objectGuid ?? Guid.Empty);
            }
        }

        /// <summary>
        /// Gets the VolunteerProfile linked to the given SecurityUser.
        /// </summary>
        public async Task<VolunteerProfile?> GetVolunteerProfileForUserAsync(SecurityUser user)
        {
            return await _schedulerDb.VolunteerProfiles
                .Where(vp => vp.linkedUserGuid == user.objectGuid)
                .Where(vp => vp.active == true && vp.deleted == false)
                .Include(vp => vp.resource)
                .Include(vp => vp.volunteerStatus)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets the default tenant GUID for the system.
        /// </summary>
        public async Task<Guid> GetDefaultTenantGuidAsync()
        {
            using var securityContext = new SecurityContext();
            var tenant = await securityContext.SecurityTenants.FirstOrDefaultAsync();
            return tenant?.objectGuid ?? Guid.Empty;
        }

        /// <summary>
        /// Validates a session token and returns the SecurityUser if valid.
        /// </summary>
        public async Task<SecurityUser?> ValidateSessionTokenAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            using (SecurityContext securityDb = new SecurityContext())
            {
                return await securityDb.SecurityUsers
                    .Where(u => u.authenticationToken == token)
                    .Where(u => u.active == true && u.deleted == false)
                    .Where(u => u.authenticationTokenExpiry != null && u.authenticationTokenExpiry > DateTime.UtcNow)
                    .FirstOrDefaultAsync();
            }
        }
    }
}
