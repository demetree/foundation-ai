using Foundation.Scheduler.Database;
using Foundation.Security;
using Foundation.Security.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foundation.Scheduler.CodeGeneration.Profiles
{
    /// <summary>
    /// Orchestrates the interactive console workflow for configuring a tenant with a default profile.
    /// 
    /// Flow:
    ///   1. List tenants from SecurityContext — user picks one
    ///   2. Prompt for geography (Country, StateProvince, TimeZone) — create or pick existing
    ///   3. Prompt for Currency — create or pick existing
    ///   4. List available profiles — user picks one
    ///   5. Apply the profile
    /// </summary>
    internal static class TenantConfigurationRunner
    {
        private static readonly ITenantConfigurationProfile[] Profiles = new ITenantConfigurationProfile[]
        {
            new SmallTownProfile(),
            new HealthcareProfile(),
            new ConstructionProfile()
        };

        public static void Run()
        {
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("  Configure Tenant with Default Profile");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine();

            // ─── Step 1: Select Tenant ───────────────────────────
            Console.WriteLine("Fetching tenants from Security database...");
            Console.WriteLine();

            List<SecurityTenant> tenants;
            using (SecurityContext sc = new SecurityContext())
            {
                tenants = (from st in sc.SecurityTenants
                           where st.active && !st.deleted
                           orderby st.name
                           select st).ToList();
            }

            if (tenants.Count == 0)
            {
                Console.WriteLine("  No active tenants found in the Security database.");
                Console.WriteLine("  Please create a tenant first, then try again.");
                Console.WriteLine();
                return;
            }

            Console.WriteLine("Available Tenants:");
            for (int i = 0; i < tenants.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {tenants[i].name}  ({tenants[i].objectGuid})");
            }
            Console.WriteLine();

            int tenantIndex = PromptForSelection("Select tenant", tenants.Count);
            if (tenantIndex < 0) return;

            SecurityTenant selectedTenant = tenants[tenantIndex];
            Console.WriteLine($"  → Selected: {selectedTenant.name}");
            Console.WriteLine();

            // ─── Step 2: Geography ───────────────────────────────
            int countryId;
            int stateProvinceId;
            int timeZoneId;

            using (SchedulerContext context = new SchedulerContext())
            {
                countryId = PromptForCountry(context);
                if (countryId <= 0) return;

                stateProvinceId = PromptForStateProvince(context, countryId);
                if (stateProvinceId <= 0) return;

                timeZoneId = PromptForTimeZone(context);
                if (timeZoneId <= 0) return;
            }

            // ─── Step 3: Currency ────────────────────────────────
            int currencyId;
            using (SchedulerContext context = new SchedulerContext())
            {
                currencyId = PromptForCurrency(context, selectedTenant.objectGuid);
                if (currencyId <= 0) return;
            }

            // ─── Step 4: Select Profile ──────────────────────────
            Console.WriteLine();
            Console.WriteLine("Available Configuration Profiles:");
            for (int i = 0; i < Profiles.Length; i++)
            {
                Console.WriteLine($"  {i + 1}. {Profiles[i].Name}");
                Console.WriteLine($"     {Profiles[i].Description}");
            }
            Console.WriteLine();

            int profileIndex = PromptForSelection("Select profile", Profiles.Length);
            if (profileIndex < 0) return;

            ITenantConfigurationProfile selectedProfile = Profiles[profileIndex];
            Console.WriteLine($"  → Selected: {selectedProfile.Name}");
            Console.WriteLine();

            // ─── Step 5: Apply ───────────────────────────────────
            TenantConfigurationContext config = new TenantConfigurationContext
            {
                TenantGuid = selectedTenant.objectGuid,
                TenantName = selectedTenant.name,
                CountryId = countryId,
                StateProvinceId = stateProvinceId,
                TimeZoneId = timeZoneId,
                CurrencyId = currencyId
            };

            Console.WriteLine($"Configuring '{selectedTenant.name}' with '{selectedProfile.Name}' profile...");
            Console.WriteLine();

            using (SchedulerContext context = new SchedulerContext())
            {
                selectedProfile.Apply(context, config);
            }

            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("  Configuration complete!");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine();
        }


        // ─────────────────────────────────────────────────────────
        // Geography & Currency prompts
        // ─────────────────────────────────────────────────────────

        private static int PromptForCountry(SchedulerContext context)
        {
            var existing = context.Countries.Where(c => c.active && !c.deleted).OrderBy(c => c.name).ToList();

            Console.WriteLine("─── Country ───");
            if (existing.Count > 0)
            {
                Console.WriteLine("Existing countries:");
                for (int i = 0; i < existing.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {existing[i].name} ({existing[i].abbreviation})");
                }
                Console.WriteLine($"  {existing.Count + 1}. [Create new]");
                Console.WriteLine();

                int choice = PromptForSelection("Select country", existing.Count + 1);
                if (choice < 0) return -1;

                if (choice < existing.Count)
                    return existing[choice].id;
            }

            // Create new
            Console.Write("  Country name: ");
            string name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name)) { Console.WriteLine("  Cancelled."); return -1; }

            Console.Write("  Abbreviation (e.g. CA, US): ");
            string abbr = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(abbr)) { Console.WriteLine("  Cancelled."); return -1; }

            Country country = new Country
            {
                name = name,
                description = name,
                abbreviation = abbr,
                sequence = existing.Count + 1,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            context.Countries.Add(country);
            context.SaveChanges();
            Console.WriteLine($"  ✓ Created country: {name}");
            return country.id;
        }


        private static int PromptForStateProvince(SchedulerContext context, int countryId)
        {
            var existing = context.StateProvinces.Where(sp => sp.countryId == countryId && sp.active && !sp.deleted).OrderBy(sp => sp.name).ToList();

            Console.WriteLine("─── State / Province ───");
            if (existing.Count > 0)
            {
                Console.WriteLine("Existing states/provinces for this country:");
                for (int i = 0; i < existing.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {existing[i].name} ({existing[i].abbreviation})");
                }
                Console.WriteLine($"  {existing.Count + 1}. [Create new]");
                Console.WriteLine();

                int choice = PromptForSelection("Select state/province", existing.Count + 1);
                if (choice < 0) return -1;

                if (choice < existing.Count)
                    return existing[choice].id;
            }

            // Create new
            Console.Write("  State/Province name: ");
            string name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name)) { Console.WriteLine("  Cancelled."); return -1; }

            Console.Write("  Abbreviation (e.g. NL, ON, NY): ");
            string abbr = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(abbr)) { Console.WriteLine("  Cancelled."); return -1; }

            StateProvince sp = new StateProvince
            {
                name = name,
                description = name,
                abbreviation = abbr,
                countryId = countryId,
                sequence = existing.Count + 1,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            context.StateProvinces.Add(sp);
            context.SaveChanges();
            Console.WriteLine($"  ✓ Created state/province: {name}");
            return sp.id;
        }


        private static int PromptForTimeZone(SchedulerContext context)
        {
            var existing = context.TimeZones.Where(tz => tz.active && !tz.deleted).OrderBy(tz => tz.name).ToList();

            Console.WriteLine("─── Time Zone ───");
            if (existing.Count > 0)
            {
                Console.WriteLine("Existing time zones:");
                for (int i = 0; i < existing.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {existing[i].name} — {existing[i].ianaTimeZone} (UTC{existing[i].standardUTCOffsetHours:+0.#;-0.#})");
                }
                Console.WriteLine($"  {existing.Count + 1}. [Create new]");
                Console.WriteLine();

                int choice = PromptForSelection("Select time zone", existing.Count + 1);
                if (choice < 0) return -1;

                if (choice < existing.Count)
                    return existing[choice].id;
            }

            // Create new
            Console.Write("  Time zone display name (e.g. St. John's): ");
            string name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name)) { Console.WriteLine("  Cancelled."); return -1; }

            Console.Write("  IANA time zone ID (e.g. America/St_Johns): ");
            string iana = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(iana)) { Console.WriteLine("  Cancelled."); return -1; }

            Console.Write("  Abbreviation (e.g. NST, EST, PST): ");
            string abbr = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(abbr)) { Console.WriteLine("  Cancelled."); return -1; }

            Console.Write("  Standard UTC offset hours (e.g. -3.5): ");
            string offsetStr = Console.ReadLine()?.Trim();
            if (!float.TryParse(offsetStr, out float offset)) { Console.WriteLine("  Invalid offset. Cancelled."); return -1; }

            Console.Write("  Supports daylight savings? (y/n): ");
            string dstStr = Console.ReadLine()?.Trim()?.ToLower();
            bool supportsDst = dstStr == "y" || dstStr == "yes";

            float dstOffset = offset;
            string dstAbbr = abbr;
            if (supportsDst)
            {
                Console.Write("  DST UTC offset hours (e.g. -2.5): ");
                string dstOffsetStr = Console.ReadLine()?.Trim();
                if (float.TryParse(dstOffsetStr, out float parsedDstOffset))
                    dstOffset = parsedDstOffset;

                Console.Write("  DST abbreviation (e.g. NDT, EDT, PDT): ");
                dstAbbr = Console.ReadLine()?.Trim() ?? abbr;
            }

            Database.TimeZone tz = new Database.TimeZone
            {
                name = name,
                description = name,
                ianaTimeZone = iana,
                abbreviation = abbr,
                standardUTCOffsetHours = offset,
                dstUTCOffsetHours = dstOffset,
                supportsDaylightSavings = supportsDst,
                abbreviationDaylightSavings = supportsDst ? dstAbbr : null,
                sequence = existing.Count + 1,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            context.TimeZones.Add(tz);
            context.SaveChanges();
            Console.WriteLine($"  ✓ Created time zone: {name} ({iana})");
            return tz.id;
        }


        private static int PromptForCurrency(SchedulerContext context, Guid tenantGuid)
        {
            var existing = context.Currencies.Where(c => c.active && !c.deleted).OrderBy(c => c.name).ToList();

            Console.WriteLine("─── Currency ───");
            if (existing.Count > 0)
            {
                Console.WriteLine("Existing currencies:");
                for (int i = 0; i < existing.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {existing[i].name} ({existing[i].code})");
                }
                Console.WriteLine($"  {existing.Count + 1}. [Create new]");
                Console.WriteLine();

                int choice = PromptForSelection("Select currency", existing.Count + 1);
                if (choice < 0) return -1;

                if (choice < existing.Count)
                    return existing[choice].id;
            }

            // Create new
            Console.Write("  Currency name (e.g. Canadian): ");
            string name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name)) { Console.WriteLine("  Cancelled."); return -1; }

            Console.Write("  Currency code (e.g. CAD, USD): ");
            string code = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(code)) { Console.WriteLine("  Cancelled."); return -1; }

            Currency currency = new Currency
            {
                name = name,
                description = name + " Dollars",
                code = code,
                color = "#1cb02d",
                isDefault = true,
                sequence = existing.Count + 1,
                tenantGuid = tenantGuid,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            context.Currencies.Add(currency);
            context.SaveChanges();
            Console.WriteLine($"  ✓ Created currency: {name} ({code})");
            return currency.id;
        }


        // ─────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────

        private static int PromptForSelection(string prompt, int max)
        {
            Console.Write($"{prompt} [1-{max}]: ");
            string input = Console.ReadLine()?.Trim();

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= max)
            {
                return choice - 1;  // zero-based
            }

            Console.WriteLine("  Invalid selection. Cancelled.");
            return -1;
        }
    }
}
