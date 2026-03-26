using System;

namespace Foundation.Scheduler.CodeGeneration.Profiles
{
    /// <summary>
    /// Holds the user-selected context values that all profiles need —
    /// tenant identity, geography IDs, and currency ID.
    /// Populated by <see cref="TenantConfigurationRunner"/> before a profile is applied.
    /// </summary>
    internal class TenantConfigurationContext
    {
        /// <summary>Guid of the SecurityTenant that was selected.</summary>
        public Guid TenantGuid { get; set; }

        /// <summary>Display name of the tenant (from SecurityTenant.name).</summary>
        public string TenantName { get; set; }

        /// <summary>Scheduler.Country.id — created or found during the geography prompt.</summary>
        public int CountryId { get; set; }

        /// <summary>Scheduler.StateProvince.id — created or found during the geography prompt.</summary>
        public int StateProvinceId { get; set; }

        /// <summary>Scheduler.TimeZone.id — created or found during the geography prompt.</summary>
        public int TimeZoneId { get; set; }

        /// <summary>Scheduler.Currency.id — created or found during the currency prompt.</summary>
        public int CurrencyId { get; set; }
    }
}
