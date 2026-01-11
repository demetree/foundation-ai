using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Foundation.Security
{
    public class TokenLogic
    {
        public const string DISABLE_CROSS_SITE_FORGERY_CONFIGURATION_SETTING_NAME = "DisableCrossSiteForgeryLogic";
        private static Object idleInactivateRoot = new Object();

        public enum EntityDataTokenTrustLevel
        {
            Read = 1,           // To read from the entity.  Allows cascade validated for other entities in the same module, to allow things like the child panes to populate when the detail screen draws
            Write = 2           // To write to the entity.  Wil not validate unless the entity is exact, unlike the read mode which is gracious to reading across entities in the same module.
        }

        public enum EntityDataTokenEventTypes
        {
            ReadFromEntity = 1,
            CascadeValidatedReadFromEntity = 2,
            WriteToEntity = 3,
            CascadeValidatedWriteToEntity = 4,
            ReuseExistingToken = 5
        }

        /*
         * 
         *   The purpose of this is to generate or retrieve an active entity data token to embed in the HTML so that the Web API calls can provide it to the service layer to validate the data request.
         *  
         */
        public static EntityDataToken GetEntityDataToken(SecurityUser securityUser, string module, string entity, string authenticationToken, string sessionId, string comments = null)
        {
            if (Foundation.Configuration.GetBooleanConfigurationSetting(DISABLE_CROSS_SITE_FORGERY_CONFIGURATION_SETTING_NAME, false) == true)
            {
                return null;
            }

            using (SecurityContext db = new SecurityContext())
            {
                Foundation.Cache.MemoryCacheManager mcm = new Foundation.Cache.MemoryCacheManager();

                int moduleId = 0;

                string moduleCacheKey = "TL_CEDT_Module_" + module;
                if (mcm.IsSet(moduleCacheKey) == true)
                {
                    moduleId = mcm.Get<int>(moduleCacheKey);
                }
                else
                {
                    moduleId = (from x in db.Modules where x.name == module select x.id).FirstOrDefault();
                    mcm.Set(moduleCacheKey, moduleId, 1440);
                }


                EntityDataToken edt = null;

                //
                // First, see if there is a record in the memory cache for the keys provided
                //
                string edtCacheKey = "TL_EDT_" + securityUser.accountName + "_" + module + "_ " + entity + "_" + authenticationToken;

                if (mcm.IsSet(edtCacheKey) == true)
                {
                    edt = mcm.Get<EntityDataToken>(edtCacheKey);
                }


                //
                //  If we don't find it in the memory cache, try the database
                //
                if (edt == null)
                {
                    edt = (from x in db.EntityDataTokens
                           where
                           x.securityUserId == securityUser.id &&
                           x.entity == entity &&
                           x.moduleId == moduleId &&
                           x.authenticationToken == authenticationToken &&
                           x.sessionId == sessionId &&
                           x.active == true &&
                           x.deleted == false
                           orderby x.timeStamp descending
                           select x).FirstOrDefault();
                }


                //
                // If we already have an EDT for this key, then we use it and we also record the action as an event on the token.
                //
                // If we don't have one yet, then we create it
                //
                if (edt != null)
                {
                    EntityDataTokenEvent edte = new EntityDataTokenEvent();

                    edte.entityDataTokenId = edt.id;
                    edte.comments = comments;
                    edte.timeStamp = DateTime.UtcNow;
                    edte.entityDataTokenEventTypeId = (int)EntityDataTokenEventTypes.ReuseExistingToken;
                    edte.active = true;
                    edte.deleted = false;

                    db.EntityDataTokenEvents.Add(edte);

                    db.SaveChanges();
                }
                else
                {
                    edt = new EntityDataToken();

                    edt.securityUserId = securityUser.id;
                    edt.moduleId = moduleId;
                    edt.entity = entity;
                    edt.token = Guid.NewGuid().ToString();
                    edt.authenticationToken = authenticationToken;
                    edt.sessionId = sessionId;

                    edt.timeStamp = DateTime.UtcNow;
                    edt.comments = comments;
                    edt.active = true;
                    edt.deleted = false;

                    db.EntityDataTokens.Add(edt);

                    db.SaveChanges();

                    //
                    // Put this in the cache for an hour
                    //
                    mcm.Set(edtCacheKey, edt, 60);
                }

                return edt;
            }
        }


        public async static Task<bool> ValidateEntityDataTokenAsync(EntityDataTokenTrustLevel trustLevel, string entityDataToken, SecurityUser securityUser, string module, string entity, string authenticationToken, string comments)      // this changes session ID to authentication token, which is stored in the forms auth ticket.  Session ID becomes reference only field.
        {
            if (Foundation.Configuration.GetBooleanConfigurationSetting(DISABLE_CROSS_SITE_FORGERY_CONFIGURATION_SETTING_NAME, false) == true)
            {
                // If the disable cross site forgery setting is on, then always return true to pretend as though the token was validated
                return true;
            }

            bool output = false;

            using (SecurityContext db = new SecurityContext())
            {
                Foundation.Cache.MemoryCacheManager mcm = new Foundation.Cache.MemoryCacheManager();

                int moduleId = 0;

                string moduleCacheKey = "TL_CEDT_Module_" + module;
                if (mcm.IsSet(moduleCacheKey) == true)
                {
                    moduleId = mcm.Get<int>(moduleCacheKey);
                }
                else
                {
                    moduleId = await (from x in db.Modules where x.name == module select x.id).FirstOrDefaultAsync();
                    mcm.Set(moduleCacheKey, moduleId, 1440);
                }


                EntityDataToken edt = null;

                //
                // First, see if there is a record in the memory cache for the keys provided.  The method that generated the key will have created this cache entry, and it may still be there.
                //
                string edtCacheKey = "TL_EDT_" + securityUser.accountName + "_" + module + "_ " + entity + "_" + authenticationToken;

                if (mcm.IsSet(edtCacheKey) == true)
                {
                    edt = mcm.Get<EntityDataToken>(edtCacheKey);
                }


                //
                //  If we don't find it in the memory cache, try in the database.  Search on all keys.
                //
                if (edt == null)
                {
                    edt = await (from x in db.EntityDataTokens
                                 where
                                 x.securityUserId == securityUser.id &&
                                 x.token == entityDataToken &&
                                 x.entity == entity &&
                                 x.moduleId == moduleId &&
                                 x.authenticationToken == authenticationToken &&
                                 x.active == true &&
                                 x.deleted == false
                                 orderby x.timeStamp descending
                                 select x)
                           .FirstOrDefaultAsync();
                }


                //
                // Do we have a token match?
                //
                if (edt != null &&
                    edt.token == entityDataToken)   // Need to test the token here because the EDT might have come from the cache instead of the DB read 
                {
                    output = true;

                    //
                    // Record the sucessful token validation
                    //
                    EntityDataTokenEvent edte = new EntityDataTokenEvent();

                    edte.entityDataTokenId = edt.id;
                    edte.comments = comments;
                    edte.timeStamp = DateTime.UtcNow;

                    if (trustLevel == EntityDataTokenTrustLevel.Write)
                    {
                        edte.entityDataTokenEventTypeId = (int)EntityDataTokenEventTypes.WriteToEntity;
                    }
                    else
                    {
                        edte.entityDataTokenEventTypeId = (int)EntityDataTokenEventTypes.ReadFromEntity;
                    }

                    edte.active = true;
                    edte.deleted = false;
                    db.EntityDataTokenEvents.Add(edte);

                    await db.SaveChangesAsync();
                }
                else
                {
                    //
                    // Check if this combination will work with the 2nd most recent Entity data token filed.  The only difference being the authentication token.
                    //
                    // This is to support timout scenarios where a timeout occurs, and a new login process creates a new authentication ticket, but keeps the same session.
                    //


                    //
                    // Get the most recent Entity Data Token by matching on all but the authentication token and entity data token
                    //
                    // Then see if this newest record has the entity data token that we're looking for.
                    // 
                    EntityDataToken mostRecentEntityDataTokenMatchingOnAllButAuthenticationTokenAndEntityDataToken = await (from x in db.EntityDataTokens
                                                                                                                            where
                                                                                                                            x.securityUserId == securityUser.id &&
                                                                                                                            x.entity == entity &&
                                                                                                                            x.moduleId == moduleId &&
                                                                                                                            x.active == true &&
                                                                                                                            x.deleted == false
                                                                                                                            orderby x.timeStamp descending
                                                                                                                            select x)
                                                                                                                      .FirstOrDefaultAsync();

                    //
                    // Does the most recent entity data token record have the correct entity token value that we're looking for?
                    //
                    if (mostRecentEntityDataTokenMatchingOnAllButAuthenticationTokenAndEntityDataToken != null)
                    {
                        if (mostRecentEntityDataTokenMatchingOnAllButAuthenticationTokenAndEntityDataToken.token == entityDataToken)
                        {
                            // we have a match. Output becomes true, and we log the authentication.

                            output = true;

                            //
                            // Record the sucessful token validation
                            //
                            EntityDataTokenEvent edte = new EntityDataTokenEvent();

                            edte.entityDataTokenId = mostRecentEntityDataTokenMatchingOnAllButAuthenticationTokenAndEntityDataToken.id;
                            edte.comments = "Using most recently filed Entity Data Token due to Authentication Token mismatch, and granting exception for this request.  Authentication token provided for validation is '" + authenticationToken + "'.  ";
                            edte.comments += comments;


                            // just in case.
                            if (edte.comments.Length > 1000)
                            {
                                edte.comments = edte.comments.Substring(0, 1000);
                            }

                            edte.timeStamp = DateTime.UtcNow;

                            if (trustLevel == EntityDataTokenTrustLevel.Write)
                            {
                                edte.entityDataTokenEventTypeId = (int)EntityDataTokenEventTypes.WriteToEntity;
                            }
                            else
                            {
                                edte.entityDataTokenEventTypeId = (int)EntityDataTokenEventTypes.ReadFromEntity;
                            }

                            edte.active = true;
                            edte.deleted = false;
                            db.EntityDataTokenEvents.Add(edte);

                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            // we don't have a match. Output stays false.
                        }
                    }
                }


                //
                // If the output is still false, then broaden the search for entities in the same module to allow for nested list access in screens where the entity wasn't explicitly used to generate the entity data token.
                //
                if (output == false)
                {
                    if (trustLevel == EntityDataTokenTrustLevel.Read)
                    {
                        //
                        // We have no match on the main keys.
                        // 
                        // If we are in the reader trust level, we can broaded the search to allow for module wide entity validation to find another entity that has this token, and grant the read permission there too.
                        //
                        // This is used for child detail pane content loading for the detail screens
                        //
                        EntityDataToken crossEntityEDT = await (from x in db.EntityDataTokens
                                                                where
                                                                x.securityUserId == securityUser.id &&
                                                                x.token == entityDataToken &&
                                                                x.moduleId == moduleId &&
                                                                x.authenticationToken == authenticationToken &&
                                                                x.active == true &&
                                                                x.deleted == false
                                                                orderby x.timeStamp descending
                                                                select x)
                                                          .FirstOrDefaultAsync();

                        if (crossEntityEDT != null)
                        {
                            output = true;

                            //
                            // Record the sucessful cascade token validation
                            //
                            EntityDataTokenEvent edte = new EntityDataTokenEvent();

                            edte.entityDataTokenId = crossEntityEDT.id;
                            edte.comments = comments;
                            edte.timeStamp = DateTime.UtcNow;
                            edte.entityDataTokenEventTypeId = (int)EntityDataTokenEventTypes.CascadeValidatedReadFromEntity;
                            edte.active = true;
                            edte.deleted = false;
                            db.EntityDataTokenEvents.Add(edte);

                            await db.SaveChangesAsync();
                        }
                    }
                    else if (trustLevel == EntityDataTokenTrustLevel.Write)
                    {
                        //
                        // We have no match on the main keys.
                        // 
                        // If we are in the write trust level, we can broaden the search to allow for module wide entity validation to find another entity that has this token, and grant the write permission there too.
                        //
                        // This is used for child detail pane content loading for the detail screens
                        //
                        EntityDataToken crossEntityEDT = await (from x in db.EntityDataTokens
                                                                where
                                                                x.securityUserId == securityUser.id &&
                                                                x.token == entityDataToken &&
                                                                x.moduleId == moduleId &&
                                                                x.authenticationToken == authenticationToken &&
                                                                x.active == true &&
                                                                x.deleted == false
                                                                orderby x.timeStamp descending
                                                                select x)
                                                          .FirstOrDefaultAsync();

                        if (crossEntityEDT != null)
                        {
                            output = true;

                            //
                            // Record the successful cascade token validation
                            //
                            EntityDataTokenEvent edte = new EntityDataTokenEvent();

                            edte.entityDataTokenId = crossEntityEDT.id;
                            edte.comments = comments;
                            edte.timeStamp = DateTime.UtcNow;
                            edte.entityDataTokenEventTypeId = (int)EntityDataTokenEventTypes.CascadeValidatedWriteToEntity;
                            edte.active = true;
                            edte.deleted = false;
                            db.EntityDataTokenEvents.Add(edte);

                            await db.SaveChangesAsync();
                        }
                    }
                }

                return output;
            }
        }


        public static bool ValidateEntityDataToken(EntityDataTokenTrustLevel trustLevel, string entityDataToken, SecurityUser securityUser, string module, string entity, string authenticationToken, string comments)      // this changes session ID to authentication token, which is stored in the forms auth ticket.  Session ID becomes reference only field.
        {
            if (Foundation.Configuration.GetBooleanConfigurationSetting(DISABLE_CROSS_SITE_FORGERY_CONFIGURATION_SETTING_NAME, false) == true)
            {
                // If the disable cross site forgery setting is on, then always return true to pretend as though the token was validated
                return true;
            }

            bool output = false;

            using (SecurityContext db = new SecurityContext())
            {
                Foundation.Cache.MemoryCacheManager mcm = new Foundation.Cache.MemoryCacheManager();

                int moduleId = 0;

                string moduleCacheKey = "TL_CEDT_Module_" + module;
                if (mcm.IsSet(moduleCacheKey) == true)
                {
                    moduleId = mcm.Get<int>(moduleCacheKey);
                }
                else
                {
                    moduleId = (from x in db.Modules where x.name == module select x.id).FirstOrDefault();
                    mcm.Set(moduleCacheKey, moduleId, 1440);
                }


                EntityDataToken edt = null;

                //
                // First, see if there is a record in the memory cache for the keys provided.  The method that generated the key will have created this cache entry, and it may still be there.
                //
                string edtCacheKey = "TL_EDT_" + securityUser.accountName + "_" + module + "_ " + entity + "_" + authenticationToken;

                if (mcm.IsSet(edtCacheKey) == true)
                {
                    edt = mcm.Get<EntityDataToken>(edtCacheKey);
                }


                //
                //  If we don't find it in the memory cache, try in the database.  Search on all keys.
                //
                if (edt == null)
                {
                    edt = (from x in db.EntityDataTokens
                           where
                           x.securityUserId == securityUser.id &&
                           x.token == entityDataToken &&
                           x.entity == entity &&
                           x.moduleId == moduleId &&
                           x.authenticationToken == authenticationToken &&
                           x.active == true &&
                           x.deleted == false
                           orderby x.timeStamp descending
                           select x).FirstOrDefault();
                }


                //
                // Do we have a token match?
                //
                if (edt != null &&
                    edt.token == entityDataToken)   // Need to test the token here because the EDT might have come from the cache instead of the DB read 
                {
                    output = true;

                    //
                    // Record the successful token validation
                    //
                    EntityDataTokenEvent edte = new EntityDataTokenEvent();

                    edte.entityDataTokenId = edt.id;
                    edte.comments = comments;
                    edte.timeStamp = DateTime.UtcNow;

                    if (trustLevel == EntityDataTokenTrustLevel.Write)
                    {
                        edte.entityDataTokenEventTypeId = (int)EntityDataTokenEventTypes.WriteToEntity;
                    }
                    else
                    {
                        edte.entityDataTokenEventTypeId = (int)EntityDataTokenEventTypes.ReadFromEntity;
                    }

                    edte.active = true;
                    edte.deleted = false;
                    db.EntityDataTokenEvents.Add(edte);

                    db.SaveChanges();
                }
                else
                {
                    //
                    // Check if this combination will work with the 2nd most recent Entity data token filed.  The only difference being the authentication token.
                    //
                    // This is to support timout scenarios where a timeout occurs, and a new login process creates a new authentication ticket, but keeps the same session.
                    //


                    //
                    // Get the most recent Entity Data Token by matching on all but the authentication token and entity data token
                    //
                    // Then see if this newest record has the entity data token that we're looking for.
                    // 
                    EntityDataToken mostRecentEntityDataTokenMatchingOnAllButAuthenticationTokenAndEntityDataToken = (from x in db.EntityDataTokens
                                                                                                                      where
                                                                                                                      x.securityUserId == securityUser.id &&
                                                                                                                      x.entity == entity &&
                                                                                                                      x.moduleId == moduleId &&
                                                                                                                      x.active == true &&
                                                                                                                      x.deleted == false
                                                                                                                      orderby x.timeStamp descending
                                                                                                                      select x).FirstOrDefault();

                    //
                    // Does the most recent entity data token record have the correct entity token value that we're looking for?
                    //
                    if (mostRecentEntityDataTokenMatchingOnAllButAuthenticationTokenAndEntityDataToken != null)
                    {
                        if (mostRecentEntityDataTokenMatchingOnAllButAuthenticationTokenAndEntityDataToken.token == entityDataToken)
                        {
                            // we have a match. Output becomes true, and we log the authentication.

                            output = true;

                            //
                            // Record the sucessful token validation
                            //
                            EntityDataTokenEvent edte = new EntityDataTokenEvent();

                            edte.entityDataTokenId = mostRecentEntityDataTokenMatchingOnAllButAuthenticationTokenAndEntityDataToken.id;
                            edte.comments = "Using most recently filed Entity Data Token due to Authentication Token mismatch, and granting exception for this request.  Authentication token provided for validation is '" + authenticationToken + "'.  ";
                            edte.comments += comments;


                            // just in case.
                            if (edte.comments.Length > 1000)
                            {
                                edte.comments = edte.comments.Substring(0, 1000);
                            }

                            edte.timeStamp = DateTime.UtcNow;

                            if (trustLevel == EntityDataTokenTrustLevel.Write)
                            {
                                edte.entityDataTokenEventTypeId = (int)EntityDataTokenEventTypes.WriteToEntity;
                            }
                            else
                            {
                                edte.entityDataTokenEventTypeId = (int)EntityDataTokenEventTypes.ReadFromEntity;
                            }

                            edte.active = true;
                            edte.deleted = false;
                            db.EntityDataTokenEvents.Add(edte);

                            db.SaveChanges();
                        }
                        else
                        {
                            // we don't have a match. Output stays false.
                        }
                    }
                }


                //
                // If the output is still false, then broaden the search for entities in the same module to allow for nested list access in screens where the entity wasn't explicitly used to generate the entity data token.
                //
                if (output == false)
                {
                    if (trustLevel == EntityDataTokenTrustLevel.Read)
                    {
                        //
                        // We have no match on the main keys.
                        // 
                        // If we are in the reader trust level, we can broaded the search to allow for module wide entity validation to find another entity that has this token, and grant the read permission there too.
                        //
                        // This is used for child detail pane content loading for the detail screens
                        //
                        EntityDataToken crossEntityEDT = (from x in db.EntityDataTokens
                                                          where
                                                          x.securityUserId == securityUser.id &&
                                                          x.token == entityDataToken &&
                                                          x.moduleId == moduleId &&
                                                          x.authenticationToken == authenticationToken &&
                                                          x.active == true &&
                                                          x.deleted == false
                                                          orderby x.timeStamp descending
                                                          select x).FirstOrDefault();

                        if (crossEntityEDT != null)
                        {
                            output = true;

                            //
                            // Record the sucessful cascade token validation
                            //
                            EntityDataTokenEvent edte = new EntityDataTokenEvent();

                            edte.entityDataTokenId = crossEntityEDT.id;
                            edte.comments = comments;
                            edte.timeStamp = DateTime.UtcNow;
                            edte.entityDataTokenEventTypeId = (int)EntityDataTokenEventTypes.CascadeValidatedReadFromEntity;
                            edte.active = true;
                            edte.deleted = false;
                            db.EntityDataTokenEvents.Add(edte);

                            db.SaveChanges();
                        }
                    }
                    else if (trustLevel == EntityDataTokenTrustLevel.Write)
                    {
                        //
                        // We have no match on the main keys.
                        // 
                        // If we are in the write trust level, we can broaden the search to allow for module wide entity validation to find another entity that has this token, and grant the write permission there too.
                        //
                        // This is used for child detail pane content loading for the detail screens
                        //
                        EntityDataToken crossEntityEDT = (from x in db.EntityDataTokens
                                                          where
                                                          x.securityUserId == securityUser.id &&
                                                          x.token == entityDataToken &&
                                                          x.moduleId == moduleId &&
                                                          x.authenticationToken == authenticationToken &&
                                                          x.active == true &&
                                                          x.deleted == false
                                                          orderby x.timeStamp descending
                                                          select x).FirstOrDefault();

                        if (crossEntityEDT != null)
                        {
                            output = true;

                            //
                            // Record the successful cascade token validation
                            //
                            EntityDataTokenEvent edte = new EntityDataTokenEvent();

                            edte.entityDataTokenId = crossEntityEDT.id;
                            edte.comments = comments;
                            edte.timeStamp = DateTime.UtcNow;
                            edte.entityDataTokenEventTypeId = (int)EntityDataTokenEventTypes.CascadeValidatedWriteToEntity;
                            edte.active = true;
                            edte.deleted = false;
                            db.EntityDataTokenEvents.Add(edte);

                            db.SaveChanges();
                        }
                    }
                }

                return output;
            }
        }


        public static void DisableIdleEntityDataTokens(int idleMinutes)
        {
            if (Foundation.Configuration.GetBooleanConfigurationSetting(TokenLogic.DISABLE_CROSS_SITE_FORGERY_CONFIGURATION_SETTING_NAME, false) == true)
            {
                return;
            }

            lock (idleInactivateRoot)
            {
                //
                // An idle entity data token is one that is active and hasn't had any activity recorded for idleMinutes
                //
                int idleCleanupPageSize = 5000;

                //
                // This would have higher performance being implmeneted as an update command or a stored proc, but then we would lose the ability to
                // be database tpe agnostic.  Therefore, until there's a need for higher performance, this general case handler is good enough.
                //
                DateTime idlecutoffTime = DateTime.UtcNow.AddMinutes(-1 * idleMinutes);

                using (SecurityContext db = new SecurityContext())
                {
                    int idleEDTCount = 0;

                    idleEDTCount = (from x in db.EntityDataTokens
                                    join y in db.EntityDataTokenEvents on x.id equals y.entityDataTokenId
                                    where
                                    x.active == true &&
                                    y.timeStamp < idlecutoffTime
                                    select x).Count();

                    Foundation.Utility.CreateAuditEvent("Idle entity data token inactivation process starting.  There are " + idleEDTCount.ToString() + " idle entity data tokens to inactivate.");

                    int remainingRecords = idleEDTCount;
                    int pageNumber = 0;

                    while (remainingRecords > 0)
                    {
                        List<EntityDataToken> idleEDTs;

                        using (SecurityContext sdb = new SecurityContext())
                        {
                            idleEDTs = (from x in sdb.EntityDataTokens
                                        join y in sdb.EntityDataTokenEvents on x.id equals y.entityDataTokenId
                                        where
                                        x.active == true &&
                                        y.timeStamp < idlecutoffTime
                                        orderby x.id
                                        select x)
                                       .Skip(pageNumber * idleCleanupPageSize)
                                       .Take(idleCleanupPageSize)
                                       .ToList();


                            pageNumber++;
                            remainingRecords -= idleEDTs.Count;

                            Foundation.Utility.CreateAuditEvent("Processing idle entity data token page " + pageNumber + " of " + idleEDTs.Count + " records and there are " + remainingRecords + " to process after this set.");

                            foreach (EntityDataToken edt in idleEDTs)
                            {
                                edt.active = false;
                            }

                            sdb.SaveChanges();
                        }
                    }

                    Foundation.Utility.CreateAuditEvent("Idle entity data token inactivation process completed.");
                }
            }
        }


        public static void DisableEntityDataTokensForAuthenticationToken(string authenticationToken)
        {
            if (Foundation.Configuration.GetBooleanConfigurationSetting(TokenLogic.DISABLE_CROSS_SITE_FORGERY_CONFIGURATION_SETTING_NAME, false) == true)
            {
                return;
            }

            //
            // Paging is probably not needed here, but doesn't hurt.
            //
            int cleanupPageSize = 1000;

            using (SecurityContext db = new SecurityContext())
            {
                int edtCount = 0;

                edtCount = (from x in db.EntityDataTokens
                            where
                            x.active == true &&
                            x.authenticationToken == authenticationToken
                            select x).Count();

                Foundation.Utility.CreateAuditEvent("Entity data token inactivation process starting for authentication token of '" + authenticationToken + "'.  There are " + edtCount.ToString() + " entity data tokens to inactivate.");

                int remainingRecords = edtCount;
                int pageNumber = 0;

                while (remainingRecords > 0)
                {
                    List<EntityDataToken> edts;

                    using (SecurityContext sdb = new SecurityContext())
                    {
                        edts = (from x in sdb.EntityDataTokens
                                where
                                x.active == true &&
                                x.authenticationToken == authenticationToken
                                orderby x.id
                                select x)
                                    .Skip(pageNumber * cleanupPageSize)
                                    .Take(cleanupPageSize)
                                    .ToList();


                        pageNumber++;
                        remainingRecords -= edts.Count;

                        Foundation.Utility.CreateAuditEvent("Processing inactivations for entity data token  for authentication token of '" + authenticationToken + "' page " + pageNumber + " of " + edts.Count + " records and there are " + remainingRecords + " to process after this set.");

                        foreach (EntityDataToken edt in edts)
                        {
                            edt.active = false;
                        }

                        sdb.SaveChanges();
                    }
                }

                Foundation.Utility.CreateAuditEvent("Entity data token inactivation  for authentication token of '" + authenticationToken + "' process completed.");
            }
        }


        public static void ClearchCachesForUser(SecurityUser securityUser)
        {
            Foundation.Cache.MemoryCacheManager mcm = new Foundation.Cache.MemoryCacheManager();

            mcm.RemoveByPattern("^TL_EDT_" + securityUser.accountName);

            return;
        }

        public static void ClearCaches()
        {
            Foundation.Cache.MemoryCacheManager mcm = new Foundation.Cache.MemoryCacheManager();

            mcm.RemoveByPattern("^TL_");

            return;
        }
    }
}
