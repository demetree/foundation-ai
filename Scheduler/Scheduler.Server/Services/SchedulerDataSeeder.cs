// AI-Developed — This file was significantly developed with AI assistance.
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Foundation.Scheduler.Database;


namespace Foundation.Scheduler.Services
{
    /// <summary>
    ///
    /// Ensures required reference data exists in the Scheduler database at startup.
    ///
    /// Called once during application startup, after schema validation but before
    /// the web server begins accepting requests. All operations are idempotent.
    ///
    /// Seeds:
    ///   - DocumentType "Invoice" (global) — required for invoice PDF archival
    ///   - DocumentType "Receipt" (global) — required for receipt PDF archival
    ///   - FinancialCategory "Cash" (per-tenant) — required for GL counter-entries
    ///   - AccountType "Asset" (global) — required for the Cash category
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    ///
    /// </summary>
    public static class SchedulerDataSeeder
    {
        /// <summary>
        /// Runs all seed operations. Safe to call multiple times — all checks are idempotent.
        /// </summary>
        public static async Task SeedRequiredDataAsync(ILogger logger)
        {
            logger.LogInformation("SchedulerDataSeeder: Starting required data seeding.");

            try
            {
                using (SchedulerContext context = new SchedulerContext())
                {
                    SeedDocumentTypes(context, logger);
                    await SeedCashCategoryForAllTenantsAsync(context, logger);
                }

                logger.LogInformation("SchedulerDataSeeder: Required data seeding complete.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SchedulerDataSeeder: Error during required data seeding.");
            }
        }


        /// <summary>
        /// Creates "Invoice" and "Receipt" DocumentTypes if they don't already exist.
        /// DocumentType is a global entity (no tenantGuid).
        /// </summary>
        private static void SeedDocumentTypes(SchedulerContext context, ILogger logger)
        {
            SeedSingleDocumentType(context, logger, "Invoice", "System-generated invoice PDF documents", 1);
            SeedSingleDocumentType(context, logger, "Receipt", "System-generated receipt PDF documents", 2);
        }


        private static void SeedSingleDocumentType(
            SchedulerContext context,
            ILogger logger,
            string name,
            string description,
            int sequence)
        {
            bool exists = context.DocumentTypes
                .Any(dt => dt.name == name && dt.active == true && dt.deleted == false);

            if (exists)
            {
                logger.LogInformation("SchedulerDataSeeder: DocumentType '{Name}' already exists.", name);
                return;
            }

            DocumentType documentType = new DocumentType
            {
                name = name,
                description = description,
                sequence = sequence,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            context.DocumentTypes.Add(documentType);
            context.SaveChanges();

            logger.LogInformation("SchedulerDataSeeder: Created DocumentType '{Name}' (id={Id}).", name, documentType.id);
        }


        /// <summary>
        /// For each active tenant, ensures a FinancialCategory with "Cash" or "Bank"
        /// in the name exists. If not, creates one named "Cash" with an Asset account type.
        /// This prevents GL postings from being silently skipped.
        /// </summary>
        private static async Task SeedCashCategoryForAllTenantsAsync(SchedulerContext context, ILogger logger)
        {
            //
            // Get or create the "Asset" account type (global entity)
            //
            AccountType assetAccountType = context.AccountTypes
                .FirstOrDefault(at => at.name == "Asset" && at.active == true && at.deleted == false);

            if (assetAccountType == null)
            {
                assetAccountType = new AccountType
                {
                    name = "Asset",
                    description = "Asset accounts (Cash, Bank, Receivables, etc.)",
                    isRevenue = false,
                    sequence = 1,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                context.AccountTypes.Add(assetAccountType);
                context.SaveChanges();

                logger.LogInformation("SchedulerDataSeeder: Created AccountType 'Asset' (id={Id}).", assetAccountType.id);
            }

            //
            // Iterate all active tenants
            //
            var tenants = context.TenantProfiles
                .Where(tp => tp.active == true && tp.deleted == false)
                .Select(tp => new { tp.tenantGuid, tp.name })
                .ToList();

            foreach (var tenant in tenants)
            {
                bool hasCashCategory = context.FinancialCategories
                    .Any(fc => fc.tenantGuid == tenant.tenantGuid
                            && fc.active == true
                            && fc.deleted == false
                            && (fc.name.Contains("Cash") || fc.name.Contains("Bank")));

                if (hasCashCategory)
                {
                    logger.LogInformation(
                        "SchedulerDataSeeder: Tenant '{TenantName}' already has a Cash/Bank category.",
                        tenant.name);
                    continue;
                }

                FinancialCategory cashCategory = new FinancialCategory
                {
                    tenantGuid = tenant.tenantGuid,
                    name = "Cash",
                    description = "General cash account for GL counter-entries",
                    code = "CASH",
                    accountTypeId = assetAccountType.id,
                    isTaxApplicable = false,
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                context.FinancialCategories.Add(cashCategory);
                context.SaveChanges();

                logger.LogInformation(
                    "SchedulerDataSeeder: Created FinancialCategory 'Cash' (id={Id}) for tenant '{TenantName}'.",
                    cashCategory.id, tenant.name);
            }
        }
    }
}
