//
// SchedulerHub.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// SignalR hub for real-time scheduler event notifications.
//
// Server pushes EventsChanged signals to tenant-scoped groups so all connected
// schedulers within the same tenant see calendar updates in real time.
//
// Auto-joins the caller's tenant group on connect using auth claims.
//
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Foundation.Security.Database;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    [Authorize]
    public class SchedulerHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            //
            // Resolve tenant from the auth token's claims and auto-join a tenant-scoped group
            //
            var tenantGuid = await ResolveTenantGuidAsync();

            if (tenantGuid.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"scheduler_{tenantGuid.Value}");
            }

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Groups are auto-cleaned on disconnect by SignalR
            await base.OnDisconnectedAsync(exception);
        }


        /// <summary>
        ///
        /// Resolve the tenant GUID from the authenticated user's claims
        /// using the SecurityContext (same pattern as SecureWebAPIController).
        ///
        /// </summary>
        private async Task<Guid?> ResolveTenantGuidAsync()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? Context.User?.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return null;
                }

                if (!int.TryParse(userId, out int userIdInt))
                {
                    return null;
                }

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
