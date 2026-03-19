//
// FileManagerHub.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// SignalR hub for real-time file manager notifications.
//
// Server pushes DocumentChanged / FolderChanged / DocumentDeleted signals
// to tenant-scoped groups so all connected users within the same tenant
// see file management updates in real time.
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
    public interface IFileManagerHubClient
    {
        Task DocumentChanged(object payload);
        Task DocumentDeleted(object payload);
        Task FolderChanged(object payload);
        Task FolderDeleted(object payload);
    }


    [Authorize]
    public class FileManagerHub : Hub<IFileManagerHubClient>
    {
        public override async Task OnConnectedAsync()
        {
            var tenantGuid = await ResolveTenantGuidAsync();

            if (tenantGuid.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"filemanager_{tenantGuid.Value}");
            }

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }


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
