// AI-Developed: Scheduler-specific implementation of IMessagingUserResolver
// Ported from Catalyst's CatalystUserResolver pattern, adapted for Scheduler's SecurityUser model

using Foundation.Messaging;
using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Foundation.Scheduler.Services
{
    /// <summary>
    /// 
    /// Scheduler-specific implementation of IMessagingUserResolver.
    /// 
    /// Unlike Catalyst which has a separate User table, Scheduler maps directly from SecurityUser
    /// to MessagingUser.  SecurityUser contains all the fields needed: id, accountName, firstName,
    /// lastName, objectGuid, and tenant info via the securityTenant navigation property.
    /// 
    /// </summary>
    public class SchedulerUserResolver : IMessagingUserResolver
    {

        public SchedulerUserResolver()
        {
        }


        public async Task<MessagingUser> GetUserAsync(SecurityUser securityUser)
        {
            if (securityUser == null) return null;

            //
            // SecurityUser is already available from the controller's auth pipeline,
            // so we can map directly without an additional DB query.
            //
            return MapToMessagingUser(securityUser);
        }


        public async Task<MessagingUser> GetUserByIdAsync(int userId, Guid tenantGuid)
        {
            using (SecurityContext secDb = new SecurityContext())
            {
                SecurityUser user = await (from u in secDb.SecurityUsers
                                           where
                                           u.id == userId &&
                                           u.active == true &&
                                           u.deleted == false
                                           select u)
                                          .Include(u => u.securityTenant)
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync();

                if (user == null) return null;

                return MapToMessagingUser(user);
            }
        }


        public async Task<MessagingUser> GetUserByAccountNameAsync(string accountName, Guid tenantGuid)
        {
            using (SecurityContext secDb = new SecurityContext())
            {
                SecurityUser user = await (from u in secDb.SecurityUsers
                                           where
                                           u.accountName == accountName &&
                                           u.active == true &&
                                           u.deleted == false
                                           select u)
                                          .Include(u => u.securityTenant)
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync();

                if (user == null) return null;

                return MapToMessagingUser(user);
            }
        }


        public async Task<List<MessagingUser>> GetUsersByIdsAsync(List<int> userIds, Guid tenantGuid)
        {
            if (userIds == null || userIds.Count == 0) return new List<MessagingUser>();

            using (SecurityContext secDb = new SecurityContext())
            {
                List<SecurityUser> users = await (from u in secDb.SecurityUsers
                                                   where
                                                   userIds.Contains(u.id) &&
                                                   u.active == true &&
                                                   u.deleted == false
                                                   select u)
                                                  .Include(u => u.securityTenant)
                                                  .AsNoTracking()
                                                  .ToListAsync();

                return users.Select(u => MapToMessagingUser(u)).ToList();
            }
        }


        public async Task<List<MessagingUser>> GetAllUsersAsync(Guid tenantGuid)
        {
            using (SecurityContext secDb = new SecurityContext())
            {
                List<SecurityUser> users = await (from u in secDb.SecurityUsers
                                                   join tu in secDb.SecurityTenantUsers on u.id equals tu.securityUserId
                                                   join t in secDb.SecurityTenants on tu.securityTenantId equals t.id
                                                   where
                                                   t.objectGuid == tenantGuid &&
                                                   u.active == true &&
                                                   u.deleted == false
                                                   orderby u.firstName, u.lastName
                                                   select u)
                                                  .Include(u => u.securityTenant)
                                                  .AsNoTracking()
                                                  .ToListAsync();

                return users.Select(u => MapToMessagingUser(u)).ToList();
            }
        }


        public async Task<List<MessagingUser>> SearchUsersAsync(string searchTerm, Guid tenantGuid, int maxResults = 20)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return new List<MessagingUser>();

            using (SecurityContext secDb = new SecurityContext())
            {
                List<SecurityUser> users = await (from u in secDb.SecurityUsers
                                                   join tu in secDb.SecurityTenantUsers on u.id equals tu.securityUserId
                                                   join t in secDb.SecurityTenants on tu.securityTenantId equals t.id
                                                   where
                                                   t.objectGuid == tenantGuid &&
                                                   u.active == true &&
                                                   u.deleted == false &&
                                                   (u.firstName.Contains(searchTerm) ||
                                                    u.lastName.Contains(searchTerm) ||
                                                    u.accountName.Contains(searchTerm))
                                                   orderby u.firstName, u.lastName
                                                   select u)
                                                  .Include(u => u.securityTenant)
                                                  .AsNoTracking()
                                                  .Take(maxResults)
                                                  .ToListAsync();

                return users.Select(u => MapToMessagingUser(u)).ToList();
            }
        }


        public async Task<SecurityUser> GetSecurityUserByMessagingUserIdAsync(int messagingUserId, Guid tenantGuid)
        {
            //
            // In Scheduler, the messaging user ID IS the SecurityUser ID,
            // so we can resolve directly.
            //
            using (SecurityContext secDb = new SecurityContext())
            {
                return await (from u in secDb.SecurityUsers
                              where
                              u.id == messagingUserId &&
                              u.active == true &&
                              u.deleted == false
                              select u)
                             .Include(u => u.securityTenant)
                             .FirstOrDefaultAsync();
            }
        }


        private static MessagingUser MapToMessagingUser(SecurityUser user)
        {
            return new MessagingUser
            {
                id = user.id,
                accountName = user.accountName,
                displayName = $"{user.firstName} {user.lastName}".Trim(),
                objectGuid = user.objectGuid,
                tenantGuid = user.securityTenant?.objectGuid ?? Guid.Empty
            };
        }
    }
}
