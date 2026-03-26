using Foundation.Scheduler.Database;

namespace Foundation.Scheduler.CodeGeneration.Profiles
{
    /// <summary>
    /// Defines a tenant configuration profile that seeds sensible default
    /// lookup / configuration records into the Scheduler database.
    /// </summary>
    internal interface ITenantConfigurationProfile
    {
        /// <summary>
        /// Human-readable name of the profile shown in the menu.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Short description of what this profile is designed for.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Seeds all configuration records for the tenant described by <paramref name="config"/>.
        /// Implementations must use idempotent check-before-insert logic.
        /// </summary>
        void Apply(SchedulerContext context, TenantConfigurationContext config);
    }
}
