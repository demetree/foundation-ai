using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Foundation.Security.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// SignalR hub for real-time Rebrickable sync status and activity streaming.
    ///
    /// Server pushes events to tenant-scoped groups:
    ///   - SyncActivity:        API call logged (direction, endpoint, success, summary)
    ///   - ConnectionChanged:   connect/disconnect/re-auth state change
    ///   - TokenWarning:        token expired or health-check failure
    ///
    /// Auto-joins the caller's tenant group on connect using auth claims.
    /// </summary>
    [Authorize]
    public class RebrickableHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Resolve tenant from the auth token's claims
            var tenantGuid = await ResolveTenantGuidAsync();
            if (tenantGuid.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"rebrickable_{tenantGuid.Value}");
            }

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Groups are auto-cleaned on disconnect by SignalR
            await base.OnDisconnectedAsync(exception);
        }


        /// <summary>
        /// Resolve the tenant GUID from the authenticated user's claims
        /// using the SecurityContext (same pattern as SecureWebAPIController).
        /// </summary>
        private async Task<Guid?> ResolveTenantGuidAsync()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? Context.User?.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId)) return null;
                if (!int.TryParse(userId, out int userIdInt)) return null;

                using var db = new SecurityContext();

                var securityUser = await db.SecurityUsers
                    .Include(u => u.securityTenant)
                    .FirstOrDefaultAsync(u => u.id == userIdInt);

                if (securityUser?.securityTenant != null
                    && securityUser.securityTenant.active == true
                    && securityUser.securityTenant.deleted == false)
                {
                    return securityUser.securityTenant.objectGuid;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
