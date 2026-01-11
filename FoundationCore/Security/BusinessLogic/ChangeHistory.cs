using Foundation.ChangeHistory;
using Foundation.Concurrent;
using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Security
{
    /// <summary>
    /// 
    /// This class provide support methods for Change History functionality for systems that link change history records to security user records in multi tenant systems.
    /// 
    /// </summary>
    public static class ChangeHistoryMultiTenant
    {

        private static ExpiringCache<Guid, List<ChangeHistoryUser>> _userCache = new ExpiringCache<Guid, List<ChangeHistoryUser>>(60, true, false);


        /// <summary>
        /// 
        /// Use this to get a ChangeHistoryUser object for the given security user id and tenant guid. using the security system's securityUser table as the source of the user data.
        /// 
        /// </summary>
        /// <param name="securityUserId"></param>
        /// <param name="tenantGuid"></param>
        /// <returns></returns>
        public static async Task<ChangeHistoryUser> GetChangeHistoryUserAsync(int securityUserId, Guid tenantGuid, CancellationToken cancellationToken = default)
        {
            //
            // Get the list of users for the tenant.
            //
            List<ChangeHistoryUser> userList = await GetChangeHistoryUserListAsync(tenantGuid, cancellationToken);
            if (userList == null || userList.Count == 0)
            {
                return null;
            }
            //
            // Find the user in the list.
            //
            ChangeHistoryUser changeHistoryUser = userList.FirstOrDefault(x => x.id == securityUserId);

            return changeHistoryUser;
        }


        /// <summary>
        /// 
        /// Use this to get a list of ChangeHistoryUser objects for the given tenant guid using the security system's security tenant user table as the source of the user data.
        /// 
        /// </summary>
        /// <param name="tenantGuid"></param>
        /// <returns></returns>
        public static async Task<List<ChangeHistoryUser>> GetChangeHistoryUserListAsync(Guid tenantGuid, CancellationToken cancellationToken = default)
        {
            //
            // Have set got the data cached already?
            //
            if (_userCache.TryGetValue(tenantGuid, out List<ChangeHistoryUser> userList) == true)
            {
                return userList;
            }
            else
            {
                //
                // Get all the users that belong to the user's tenant.
                //
                using (SecurityContext context = new SecurityContext())
                {
                    List<SecurityTenantUser> tenantUsers = await (from stu in context.SecurityTenantUsers
                                                                  join st in context.SecurityTenants on stu.securityTenantId equals st.id
                                                                  where st.objectGuid == tenantGuid &&
                                                                  // Note: The usual active and deleted restrictions are not put on the users in this query to allow all users to be returned because the change history system needs to be able to access all users, not just active ones.
                                                                  st.active == true &&
                                                                  st.deleted == false
                                                                  select stu)
                                                                  .Include(x => x.securityUser)
                                                                  .ToListAsync(cancellationToken)
                                                                  .ConfigureAwait(false);


                    //
                    // Build the list of change history users from the tenant users.
                    //
                    // We expect that there will be security tenant users for the tenant that the user belongs to.
                    //
                    foreach (SecurityTenantUser tenantUser in tenantUsers)
                    {
                        if (tenantUser.securityUser != null)
                        {
                            ChangeHistoryUser changeHistoryUser = new ChangeHistoryUser
                            {
                                id = tenantUser.securityUser.id,
                                userName = tenantUser.securityUser.accountName,
                                firstName = tenantUser.securityUser.firstName,
                                middleName = tenantUser.securityUser.middleName,
                                lastName = tenantUser.securityUser.lastName
                            };

                            if (userList == null)
                            {
                                userList = new List<ChangeHistoryUser>();
                            }

                            userList.Add(changeHistoryUser);
                        }
                    }

                    _userCache.Add(tenantGuid, userList);

                    return userList;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// This class provide support methods for Change History functionality for systems without multi tenancy that link change history records to security user table records.
    /// 
    /// </summary>
    public static class ChangeHistory
    {

        private static ExpiringCache<int, List<ChangeHistoryUser>> _userCache = new ExpiringCache<int, List<ChangeHistoryUser>>(60, true, false);   // Key of 0 will always be used.


        /// <summary>
        /// 
        /// Use this to get a ChangeHistoryUser object for the given security user id and tenant guid. using the security system's securityUser table as the source of the user data.
        /// 
        /// </summary>
        /// <param name="securityUserId"></param>
        /// <param name="tenantGuid"></param>
        /// <returns></returns>
        public static async Task<ChangeHistoryUser> GetChangeHistoryUserAsync(int securityUserId, CancellationToken cancellationToken = default)
        {
            //
            // Get the list of users for the tenant.
            //
            List<ChangeHistoryUser> userList = await GetChangeHistoryUserListAsync(cancellationToken);
            if (userList == null || userList.Count == 0)
            {
                return null;
            }
            //
            // Find the user in the list.
            //
            ChangeHistoryUser changeHistoryUser = userList.FirstOrDefault(x => x.id == securityUserId);

            return changeHistoryUser;
        }


        /// <summary>
        /// 
        /// Use this to get a list of ChangeHistoryUser objects using the securityUser table as the source of the user data.  It does not reference tenants in any way.  All users in the security user table will be returned.
        /// 
        /// </summary>
        /// <param name="tenantGuid"></param>
        /// <returns></returns>
        public static async Task<List<ChangeHistoryUser>> GetChangeHistoryUserListAsync(CancellationToken cancellationToken = default)
        {
            //
            // Have set got the data cached already?
            //
            if (_userCache.TryGetValue(0, out List<ChangeHistoryUser> userList) == true)
            {
                return userList;
            }
            else
            {
                //
                // Get all the users -- Note that this does not filter by tenant.
                //
                using (SecurityContext context = new SecurityContext())
                {
                    List<SecurityUser> securityUsers = await (from su in context.SecurityUsers
                                                                  // Note: The usual active and deleted restrictions are not put on the users in this query to allow all users to be returned because the change history system needs to be able to access all users, not just active ones.
                                                              select su)
                                                             .ToListAsync(cancellationToken)
                                                             .ConfigureAwait(false);


                    //
                    // Build the list of change history users from the tenant users.
                    //
                    // We expect that there will be security tenant users for the tenant that the user belongs to.
                    //
                    foreach (SecurityUser securityUser in securityUsers)
                    {
                        if (securityUser != null)
                        {
                            ChangeHistoryUser changeHistoryUser = new ChangeHistoryUser
                            {
                                id = securityUser.id,
                                userName = securityUser.accountName,
                                firstName = securityUser.firstName,
                                middleName = securityUser.middleName,
                                lastName = securityUser.lastName
                            };

                            if (userList == null)
                            {
                                userList = new List<ChangeHistoryUser>();
                            }

                            userList.Add(changeHistoryUser);
                        }
                    }

                    _userCache.Add(0, userList);

                    return userList;
                }
            }
        }
    }
}