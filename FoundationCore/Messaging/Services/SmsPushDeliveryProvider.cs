using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Console SMS Push Delivery Provider - development stub for SMS delivery.
    /// 
    /// This implementation logs SMS messages to the console/debug output rather than
    /// actually sending them.  It serves as a placeholder that can be swapped for
    /// a real SMS provider (Twilio, AWS SNS, Azure Communication Services, etc.)
    /// without touching the PushDeliveryService.
    /// 
    /// Configuration in appsettings.json under "Messaging:Sms":
    /// 
    ///   "Messaging": {
    ///     "Sms": {
    ///       "Enabled": false,
    ///       "Provider": "console"
    ///     }
    ///   }
    /// 
    /// To integrate a real SMS provider:
    ///   1. Create a new class implementing IPushDeliveryProvider
    ///   2. Register it in DI instead of this stub
    ///   3. Update configuration with provider-specific credentials
    /// 
    /// AI-developed as part of Foundation.Messaging Phase 3B, March 2026.
    /// 
    /// </summary>
    public class SmsPushDeliveryProvider : IPushDeliveryProvider
    {
        private readonly bool _enabled;
        private readonly string _providerName;


        public string ProviderId => "sms";
        public string DisplayName => "SMS (Console Stub)";
        public bool IsEnabled => _enabled;


        public SmsPushDeliveryProvider(IConfiguration configuration)
        {
            IConfigurationSection sms = configuration?.GetSection("Messaging:Sms");

            _enabled = sms?.GetValue<bool>("Enabled") ?? false;
            _providerName = sms?.GetValue<string>("Provider") ?? "console";
        }


        /// <summary>
        /// Simulates SMS delivery by writing to the debug console.
        /// Replace this implementation with a real SMS API call when ready.
        /// </summary>
        public Task<PushDeliveryResult> DeliverAsync(PushDeliveryRequest request, CancellationToken cancellationToken = default)
        {
            if (_enabled == false)
            {
                return Task.FromResult(PushDeliveryResult.Failed(ProviderId, "SMS provider is not enabled."));
            }

            if (string.IsNullOrWhiteSpace(request?.Destination))
            {
                return Task.FromResult(PushDeliveryResult.Failed(ProviderId, "No phone number specified."));
            }

            //
            // Console stub — log the SMS that would have been sent
            //
            string truncatedBody = request.Body;
            if (truncatedBody != null && truncatedBody.Length > 160)
            {
                truncatedBody = truncatedBody.Substring(0, 157) + "...";
            }

            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════════════════════════");
            System.Diagnostics.Debug.WriteLine("  SMS DELIVERY (CONSOLE STUB)");
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════════════════════════");
            System.Diagnostics.Debug.WriteLine($"  To:      {request.Destination}");
            System.Diagnostics.Debug.WriteLine($"  From:    {request.SenderDisplayName ?? "(System)"}");
            System.Diagnostics.Debug.WriteLine($"  Tenant:  {request.TenantGuid}");
            System.Diagnostics.Debug.WriteLine($"  Body:    {truncatedBody}");
            System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════════════════════════");

            //
            // Generate a fake external ID for logging purposes
            //
            string fakeExternalId = $"sms-stub-{Guid.NewGuid():N}".Substring(0, 24);

            return Task.FromResult(PushDeliveryResult.Succeeded(ProviderId, fakeExternalId));
        }


        /// <summary>
        /// Validates that the destination looks like a phone number.
        /// Accepts formats like: +15551234567, 555-123-4567, (555) 123-4567
        /// </summary>
        public bool ValidateDestination(string destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                return false;
            }

            //
            // Strip non-numeric characters except leading +
            //
            string digitsOnly = Regex.Replace(destination.Trim(), @"[^\d+]", "");

            //
            // Must have at least 10 digits (valid phone number)
            //
            string justDigits = digitsOnly.Replace("+", "");
            return justDigits.Length >= 10 && justDigits.Length <= 15;
        }
    }
}
