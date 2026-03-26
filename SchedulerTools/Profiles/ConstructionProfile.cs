using Foundation.Scheduler.Database;
using System;
using System.Linq;

namespace Foundation.Scheduler.CodeGeneration.Profiles
{
    /// <summary>
    /// Default configuration profile for construction and technical services companies.
    /// Models service dispatch, site scheduling, crew management, and trade-specific workflows.
    /// </summary>
    internal class ConstructionProfile : ITenantConfigurationProfile
    {
        public string Name => "Construction / Technical Services";
        public string Description => "Service dispatch, site scheduling, crew management, and trade-specific workflows";

        public void Apply(SchedulerContext context, TenantConfigurationContext config)
        {
            Guid tg = config.TenantGuid;
            int created = 0;

            // ──────────────────────────────────────────────
            // Priorities
            // ──────────────────────────────────────────────
            var priorities = new[]
            {
                (Name: "Emergency", Description: "Emergency — immediate response required", Color: "#DC2626", Seq: 1),
                (Name: "High",      Description: "High Priority",                          Color: "#F59E0B", Seq: 2),
                (Name: "Normal",    Description: "Normal Priority",                        Color: "#3B82F6", Seq: 3),
                (Name: "Low",       Description: "Low Priority",                           Color: "#98948F", Seq: 4)
            };

            foreach (var p in priorities)
            {
                if (!context.Priorities.Any(x => x.name == p.Name && x.tenantGuid == tg))
                {
                    context.Priorities.Add(new Priority
                    {
                        name = p.Name,
                        description = p.Description,
                        color = p.Color,
                        sequence = p.Seq,
                        tenantGuid = tg,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    created++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Priorities ({created} records)");


            // ──────────────────────────────────────────────
            // Calendars
            // ──────────────────────────────────────────────
            created = 0;
            var calendars = new[]
            {
                (Name: "Active Jobs",   Description: "Active job schedule",           Color: "#3B82F6", IconId: 28, IsDefault: true),
                (Name: "Maintenance",   Description: "Preventive maintenance",        Color: "#10B981", IconId: 29, IsDefault: false),
                (Name: "Inspections",   Description: "Inspection and compliance",     Color: "#F59E0B", IconId: 3,  IsDefault: false)
            };

            foreach (var c in calendars)
            {
                if (!context.Calendars.Any(x => x.name == c.Name && x.tenantGuid == tg))
                {
                    context.Calendars.Add(new Calendar
                    {
                        name = c.Name,
                        description = c.Description,
                        color = c.Color,
                        iconId = c.IconId,
                        isDefault = c.IsDefault,
                        versionNumber = 0,
                        tenantGuid = tg,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    created++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Calendars ({created} records)");


            // ──────────────────────────────────────────────
            // Resource Types
            // ──────────────────────────────────────────────
            created = 0;
            var resourceTypes = new[]
            {
                (Name: "Technician",      Description: "Technician / Tradesperson",     IconId: 4,  Color: "#3B82F6", IsBillable: true,  Seq: 1),
                (Name: "Supervisor",      Description: "Site Supervisor / Foreman",     IconId: 3,  Color: "#F59E0B", IsBillable: true,  Seq: 2),
                (Name: "Apprentice",      Description: "Apprentice",                   IconId: 10, Color: "#10B981", IsBillable: true,  Seq: 3),
                (Name: "Heavy Equipment", Description: "Heavy Equipment / Machinery",  IconId: 8,  Color: "#7C3AED", IsBillable: true,  Seq: 4),
                (Name: "Vehicle",         Description: "Company Vehicle",              IconId: 8,  Color: "#2a8479", IsBillable: false, Seq: 5)
            };

            foreach (var rt in resourceTypes)
            {
                if (!context.ResourceTypes.Any(x => x.name == rt.Name && x.tenantGuid == tg))
                {
                    context.ResourceTypes.Add(new ResourceType
                    {
                        name = rt.Name,
                        description = rt.Description,
                        iconId = rt.IconId,
                        color = rt.Color,
                        isBillable = rt.IsBillable,
                        sequence = rt.Seq,
                        tenantGuid = tg,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    created++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Resource Types ({created} records)");


            // ──────────────────────────────────────────────
            // Assignment Roles
            // ──────────────────────────────────────────────
            created = 0;
            var assignmentRoles = new[]
            {
                (Name: "Lead",           Description: "Lead Technician / Crew Lead", IconId: 3,  Color: "#1D4ED8", Seq: 1),
                (Name: "Crew Member",    Description: "Crew Member",                 IconId: 4,  Color: "#3B82F6", Seq: 2),
                (Name: "Inspector",      Description: "Inspector",                   IconId: 28, Color: "#F59E0B", Seq: 3),
                (Name: "Safety Officer", Description: "Safety Officer",              IconId: 46, Color: "#DC2626", Seq: 4)
            };

            foreach (var ar in assignmentRoles)
            {
                if (!context.AssignmentRoles.Any(x => x.name == ar.Name && x.tenantGuid == tg))
                {
                    context.AssignmentRoles.Add(new AssignmentRole
                    {
                        name = ar.Name,
                        description = ar.Description,
                        iconId = ar.IconId,
                        color = ar.Color,
                        sequence = ar.Seq,
                        tenantGuid = tg,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    created++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Assignment Roles ({created} records)");


            // ──────────────────────────────────────────────
            // Client Types
            // ──────────────────────────────────────────────
            created = 0;
            var clientTypes = new[]
            {
                (Name: "Commercial",           Description: "Commercial / Business Client",    IconId: 56, Color: "#3B82F6", Seq: 1),
                (Name: "Residential",          Description: "Residential Client",              IconId: 14, Color: "#10B981", Seq: 2),
                (Name: "Government / Municipal", Description: "Government or Municipal Client", IconId: 25, Color: "#7C3AED", Seq: 3)
            };

            foreach (var ct in clientTypes)
            {
                if (!context.ClientTypes.Any(x => x.name == ct.Name && x.tenantGuid == tg))
                {
                    context.ClientTypes.Add(new ClientType
                    {
                        name = ct.Name,
                        description = ct.Description,
                        iconId = ct.IconId,
                        color = ct.Color,
                        sequence = ct.Seq,
                        tenantGuid = tg,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    created++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Client Types ({created} records)");


            // ──────────────────────────────────────────────
            // Scheduling Target Types
            // ──────────────────────────────────────────────
            created = 0;
            var targetTypes = new[]
            {
                (Name: "Job Site",              Description: "Construction or service job site",     IconId: 28, Color: "#3B82F6", Seq: 1),
                (Name: "Office / Shop",         Description: "Office, shop, or warehouse",          IconId: 56, Color: "#6B7280", Seq: 2),
                (Name: "Residential Property",  Description: "Residential property",                IconId: 14, Color: "#10B981", Seq: 3),
                (Name: "Infrastructure",        Description: "Infrastructure / municipal asset",    IconId: 51, Color: "#7C3AED", Seq: 4)
            };

            foreach (var tt in targetTypes)
            {
                if (!context.SchedulingTargetTypes.Any(x => x.name == tt.Name && x.tenantGuid == tg))
                {
                    context.SchedulingTargetTypes.Add(new SchedulingTargetType
                    {
                        name = tt.Name,
                        description = tt.Description,
                        iconId = tt.IconId,
                        color = tt.Color,
                        sequence = tt.Seq,
                        tenantGuid = tg,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    created++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Scheduling Target Types ({created} records)");


            // ──────────────────────────────────────────────
            // Rate Types
            // ──────────────────────────────────────────────
            created = 0;
            var rateTypes = new[]
            {
                (Name: "Standard",  Description: "Standard billing rate",    Color: "#3B82F6", Seq: 1),
                (Name: "Overtime",  Description: "Overtime billing rate",    Color: "#F59E0B", Seq: 2),
                (Name: "Emergency", Description: "Emergency billing rate",   Color: "#DC2626", Seq: 3)
            };

            foreach (var rt in rateTypes)
            {
                if (!context.RateTypes.Any(x => x.name == rt.Name && x.tenantGuid == tg))
                {
                    context.RateTypes.Add(new RateType
                    {
                        name = rt.Name,
                        description = rt.Description,
                        color = rt.Color,
                        sequence = rt.Seq,
                        tenantGuid = tg,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    created++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Rate Types ({created} records)");


            // ──────────────────────────────────────────────
            // Tax Codes
            // ──────────────────────────────────────────────
            created = 0;
            var taxCodes = new[]
            {
                (Name: "HST",    Description: "Harmonized Sales Tax (15%)", Code: "HST",    Rate: 15.0m, IsDefault: true,  IsExempt: false, Seq: 1),
                (Name: "Exempt", Description: "Tax exempt",                 Code: "EXEMPT", Rate: 0m,    IsDefault: false, IsExempt: true,  Seq: 2)
            };

            foreach (var tc in taxCodes)
            {
                if (!context.TaxCodes.Any(x => x.code == tc.Code && x.tenantGuid == tg))
                {
                    context.TaxCodes.Add(new TaxCode
                    {
                        name = tc.Name,
                        description = tc.Description,
                        code = tc.Code,
                        rate = tc.Rate,
                        isDefault = tc.IsDefault,
                        isExempt = tc.IsExempt,
                        sequence = tc.Seq,
                        tenantGuid = tg,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    created++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Tax Codes ({created} records)");


            // ──────────────────────────────────────────────
            // Charge Types
            // ──────────────────────────────────────────────
            created = 0;
            var chargeTypes = new[]
            {
                (Name: "Labour",               Description: "Labour / service hours",          Color: "#3B82F6", IsRevenue: true, IsTaxable: true,  Seq: 1),
                (Name: "Materials",            Description: "Materials and supplies",          Color: "#10B981", IsRevenue: true, IsTaxable: true,  Seq: 2),
                (Name: "Equipment Rental",     Description: "Equipment rental charge",         Color: "#7C3AED", IsRevenue: true, IsTaxable: true,  Seq: 3),
                (Name: "Travel / Mobilization", Description: "Travel or mobilization charge",  Color: "#F59E0B", IsRevenue: true, IsTaxable: true,  Seq: 4)
            };

            foreach (var ct in chargeTypes)
            {
                if (!context.ChargeTypes.Any(x => x.name == ct.Name && x.tenantGuid == tg))
                {
                    context.ChargeTypes.Add(new ChargeType
                    {
                        name = ct.Name,
                        description = ct.Description,
                        color = ct.Color,
                        isRevenue = ct.IsRevenue,
                        isTaxable = ct.IsTaxable,
                        currencyId = config.CurrencyId,
                        sequence = ct.Seq,
                        versionNumber = 0,
                        tenantGuid = tg,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    created++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Charge Types ({created} records)");


            // ──────────────────────────────────────────────
            // Event Types
            // ──────────────────────────────────────────────
            created = 0;
            int? labourChargeId = context.ChargeTypes
                .Where(x => x.name == "Labour" && x.tenantGuid == tg)
                .Select(x => (int?)x.id)
                .FirstOrDefault();

            var eventTypes = new[]
            {
                (Name: "Service Call",           Desc: "On-site service call",                   Color: "#3B82F6", ReqPayment: true,  IsInternal: false, Seq: 1),
                (Name: "Inspection",             Desc: "Scheduled inspection",                   Color: "#F59E0B", ReqPayment: true,  IsInternal: false, Seq: 2),
                (Name: "Preventive Maintenance", Desc: "Scheduled preventive maintenance",       Color: "#10B981", ReqPayment: true,  IsInternal: false, Seq: 3),
                (Name: "Installation",           Desc: "Equipment or system installation",       Color: "#7C3AED", ReqPayment: true,  IsInternal: false, Seq: 4),
                (Name: "Emergency Repair",       Desc: "Emergency repair or call-out",           Color: "#DC2626", ReqPayment: true,  IsInternal: false, Seq: 5)
            };

            foreach (var et in eventTypes)
            {
                if (!context.EventTypes.Any(x => x.name == et.Name && x.tenantGuid == tg))
                {
                    context.EventTypes.Add(new EventType
                    {
                        name = et.Name,
                        description = et.Desc,
                        color = et.Color,
                        requiresRentalAgreement = false,
                        requiresExternalContact = false,
                        requiresPayment = et.ReqPayment,
                        requiresDeposit = false,
                        requiresBarService = false,
                        allowsTicketSales = false,
                        isInternalEvent = et.IsInternal,
                        chargeTypeId = labourChargeId,
                        sequence = et.Seq,
                        versionNumber = 0,
                        tenantGuid = tg,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    created++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Event Types ({created} records)");


            // ──────────────────────────────────────────────
            // Crews
            // ──────────────────────────────────────────────
            created = 0;
            var crews = new[]
            {
                (Name: "Service Team A",   Description: "Service Team A",      IconId: 28, Color: "#3B82F6"),
                (Name: "Service Team B",   Description: "Service Team B",      IconId: 29, Color: "#6366F1"),
                (Name: "Maintenance Crew", Description: "Maintenance Crew",    IconId: 28, Color: "#10B981")
            };

            foreach (var cr in crews)
            {
                if (!context.Crews.Any(x => x.name == cr.Name && x.tenantGuid == tg))
                {
                    context.Crews.Add(new Crew
                    {
                        name = cr.Name,
                        description = cr.Description,
                        iconId = cr.IconId,
                        color = cr.Color,
                        versionNumber = 0,
                        tenantGuid = tg,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    created++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Crews ({created} records)");


            // ──────────────────────────────────────────────
            // Document Types (global — no tenantGuid)
            // ──────────────────────────────────────────────
            created = 0;
            var documentTypes = new[]
            {
                (Name: "Work Order",              Description: "Work order / service ticket",           Color: "#3B82F6", Seq: 1),
                (Name: "Quote / Estimate",        Description: "Quote or cost estimate",               Color: "#10B981", Seq: 2),
                (Name: "Invoice",                 Description: "Invoice document",                     Color: "#7C3AED", Seq: 3),
                (Name: "Safety Report",           Description: "Site safety report or incident log",   Color: "#DC2626", Seq: 4),
                (Name: "Inspection Certificate",  Description: "Inspection or compliance certificate", Color: "#F59E0B", Seq: 5)
            };

            foreach (var dt in documentTypes)
            {
                if (!context.DocumentTypes.Any(x => x.name == dt.Name))
                {
                    context.DocumentTypes.Add(new DocumentType
                    {
                        name = dt.Name,
                        description = dt.Description,
                        color = dt.Color,
                        sequence = dt.Seq,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    created++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Document Types ({created} records)");


            // ──────────────────────────────────────────────
            // Tenant Profile
            // ──────────────────────────────────────────────
            created = 0;
            if (!context.TenantProfiles.Any(x => x.tenantGuid == tg))
            {
                context.TenantProfiles.Add(new TenantProfile
                {
                    name = config.TenantName,
                    description = config.TenantName,
                    stateProvinceId = config.StateProvinceId,
                    countryId = config.CountryId,
                    versionNumber = 0,
                    tenantGuid = tg,
                    objectGuid = tg,
                    active = true,
                    deleted = false
                });
                created++;
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Tenant Profile ({created} records)");
        }
    }
}
