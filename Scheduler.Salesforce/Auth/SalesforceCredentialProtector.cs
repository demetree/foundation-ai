//
// SalesforceCredentialProtector.cs
//
// Provides encryption and decryption for Salesforce credentials stored in
// the SalesforceTenantLink table. Uses ASP.NET Core's Data Protection API
// with a purpose string specific to Salesforce credentials.
//
// Fields protected: sfClientSecret, sfPassword, sfSecurityToken
//
// AI Assisted Development:  This file was created with AI assistance for the
// Scheduler Salesforce integration.
//

using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;


namespace Scheduler.Salesforce.Auth
{
    /// <summary>
    ///
    /// Wraps IDataProtector with a purpose string scoped to Salesforce credentials.
    /// Handles graceful fallback when decryption fails (e.g. key rotation or
    /// unencrypted legacy values), logging a warning instead of throwing.
    ///
    /// </summary>
    public class SalesforceCredentialProtector
    {
        private const string PURPOSE = "Salesforce.Credentials.v1";

        private readonly IDataProtector _protector;
        private readonly ILogger<SalesforceCredentialProtector> _logger;


        public SalesforceCredentialProtector(
            IDataProtectionProvider dataProtectionProvider,
            ILogger<SalesforceCredentialProtector> logger)
        {
            _protector = dataProtectionProvider.CreateProtector(PURPOSE);
            _logger = logger;
        }


        /// <summary>
        ///
        /// Encrypts a plaintext credential value.
        /// Returns null if the input is null or empty.
        ///
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText) == true) return plainText;

            return _protector.Protect(plainText);
        }


        /// <summary>
        ///
        /// Decrypts a previously encrypted credential value.
        /// Returns the original value if decryption fails (graceful fallback
        /// for unencrypted legacy data or key rotation scenarios).
        ///
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText) == true) return cipherText;

            try
            {
                return _protector.Unprotect(cipherText);
            }
            catch (Exception ex)
            {
                //
                // Graceful fallback: if the value was never encrypted (pre-encryption data)
                // or the key has rotated, return the raw value and log a warning.
                // This prevents the entire sync flow from breaking during migration.
                //
                _logger.LogWarning(ex, "Failed to decrypt Salesforce credential — returning raw value (may be unencrypted legacy data)");
                return cipherText;
            }
        }
    }
}
