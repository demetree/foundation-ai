using System;
using Foundation.Security;
using Foundation.Auditor;
using Foundation.Security.Database;
using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Foundation.Concurrent;

namespace Foundation.Controllers
{
	
    public abstract class FoundationControllerBase : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        //
        // 5 Minutes to suppress repeated audit messages for - This means the same audit log message won't be created more than once every 5 minutes
        //
        private static ExpiringCache<string, bool> _auditFloodCheckerCache = new ExpiringCache<string, bool>(300);


        public FoundationControllerBase(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        protected async Task<bool> CreateAuditEventAsync(SecurityUser securityUser, AuditEngine.AuditType auditType, string message, Boolean success, string primaryKey, string beforeState, string afterState, Exception ex)
        {
            string userName = "";


            if (securityUser == null)
            {
                userName = "Unknown";
            }
            else
            {
                if (securityUser.accountName != null && securityUser.accountName.Length > 0)
                {
                    userName = securityUser.accountName;
                }
                else
                {
                    userName = "Unknown";
                }
            }

            //
            // Audit flood protection.  Return true to the caller when we have suppressed a message because it doesn't affect them.
            // 
            string floodCacheKey = $"{userName}_{auditType}_{message}_{success}_{primaryKey}_{beforeState}_{afterState}";
            if (_auditFloodCheckerCache.ContainsKey(floodCacheKey) == true)
            {
                return true;
            }
            else
            {
                _auditFloodCheckerCache.Add(floodCacheKey, true);
            }

            List<string> errors = null;

            if (ex != null)
            {
                errors = new List<string>();

                Exception subEx = ex;

                //
                // First put in the entire error written as a string
                //
                errors.Add(ex.ToString());

                while (subEx != null)
                {
                    errors.Add(subEx.Message + " - " + subEx.ToString());
                    subEx = subEx.InnerException;
                }
            }

            DateTime startTime = DateTime.UtcNow;
            DateTime stopTime = DateTime.UtcNow;


            string sessionId = null;

            string environmentName = _environment.EnvironmentName ?? "Unknown";

            string typeName = this.GetType().Name;

            AuditEngine a = Foundation.Auditor.AuditEngine.Instance;
            await a.CreateAuditEventAsync(startTime,
                stopTime,
                success,
                AuditEngine.AuditAccessType.WebBrowser,
                auditType,
                userName,
                sessionId,
                HttpContext.Request.Headers["UserHostAddress"],
                HttpContext.Request.Headers["UserAgent"],
                typeName,
                typeName,
                HttpContext.Request.Path.ToString() + HttpContext.Request.QueryString.ToString(),
                environmentName,
                primaryKey,
                System.Threading.Thread.CurrentThread.ManagedThreadId,
                message,
                beforeState,
                afterState,
                errors);

            return true;
        }


        protected static async Task<Guid> UserTenantGuidAsync(SecurityUser securityUser)
        {
            if (securityUser == null)
            {
                throw new Exception("No security user provided.");
            }
            else
            {
                return await SecurityFramework.UserTenantGuidAsync(securityUser);
            }
        }
    }
}
