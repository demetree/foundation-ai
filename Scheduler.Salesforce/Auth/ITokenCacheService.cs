//
// ITokenCacheService.cs
//
// Per-tenant token caching interface for Salesforce OAuth tokens.
//
// AI Assisted Development:  This file was created with AI assistance for the
// Scheduler Salesforce integration.
//

using System;
using System.Threading.Tasks;


namespace Scheduler.Salesforce.Auth
{
    public interface ITokenCacheService
    {
        Task<string> GetAccessTokenAsync(Guid tenantGuid);

        Task SetAccessTokenAsync(Guid tenantGuid, string token, DateTime expiration);

        Task<string> GetInstanceUrlAsync(Guid tenantGuid);

        Task SetInstanceUrlAsync(Guid tenantGuid, string instanceUrl);

        Task ClearTokenAsync(Guid tenantGuid);
    }
}
