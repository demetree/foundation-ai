//
// SalesforceConfig.cs
//
// Configuration model for per-tenant Salesforce connections.
//
// AI Assisted Development:  This file was created with AI assistance for the
// Scheduler Salesforce integration.
//

using System;


namespace Scheduler.Salesforce.Models
{
    public class SalesforceConfig
    {
        public Guid TenantGuid { get; set; }

        public string LoginUrl { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string SecurityToken { get; set; }

        public string ApiVersion { get; set; } = "v56.0";

        public string SyncDirectionFlags { get; set; } = "None";

        public int PullIntervalMinutes { get; set; } = 5;
    }
}
