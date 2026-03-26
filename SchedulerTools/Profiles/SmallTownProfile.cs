using Foundation.Scheduler.Database;
using System;
using System.Linq;

namespace Foundation.Scheduler.CodeGeneration.Profiles
{
    /// <summary>
    /// Default configuration profile for small-town operations &amp; recreation committees.
    /// Models community hall bookings, town crews, municipal scheduling, and rec events.
    /// </summary>
    internal class SmallTownProfile : ITenantConfigurationProfile
    {
        public string Name => "Small Town Operations & Recreation Committee";
        public string Description => "Community hall bookings, town crews, municipal scheduling, and rec events";

        public void Apply(SchedulerContext context, TenantConfigurationContext config)
        {
            Guid tg = config.TenantGuid;
            int created = 0;

            // ──────────────────────────────────────────────
            // Priorities
            // ──────────────────────────────────────────────
            var priorities = new[]
            {
                (Name: "High",   Description: "High Priority",   Color: "#2BF164", Seq: 1),
                (Name: "Medium", Description: "Medium Priority",  Color: "#F1A82C", Seq: 2),
                (Name: "Low",    Description: "Low Priority",     Color: "#98948F", Seq: 3)
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
                (Name: "Recreation",  Description: "Recreation Events",   Color: "#fb0909", IconId: 10, IsDefault: false),
                (Name: "Community",   Description: "Community Events",    Color: "#06b131", IconId: 8,  IsDefault: true),
                (Name: "Council",     Description: "Council Meetings",    Color: "#3632f5", IconId: 3,  IsDefault: false)
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
                (Name: "Worker",        Description: "Town Worker",                       IconId: 4,  Color: "#2889f0", IsBillable: false, Seq: 1),
                (Name: "Volunteer",     Description: "Volunteer",                         IconId: 10, Color: "#febebe", IsBillable: false, Seq: 2),
                (Name: "Councilor",     Description: "Town Councilor",                    IconId: 3,  Color: "#f29a36", IsBillable: false, Seq: 3),
                (Name: "Vehicle",       Description: "Vehicle",                           IconId: 8,  Color: "#2a8479", IsBillable: false, Seq: 4),
                (Name: "Facility",      Description: "Rentable facility or building",     IconId: 56, Color: "#6b5ce7", IsBillable: true,  Seq: 5)
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
                (Name: "Staff",     Description: "Staff Member", IconId: 28, Color: "#6389e3", Seq: 1),
                (Name: "Volunteer", Description: "Volunteer",    IconId: 29, Color: "#00b825", Seq: 2)
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
                (Name: "Resident", Description: "Local Resident",    IconId: 14, Color: "#393ed5", Seq: 1),
                (Name: "External", Description: "External Client",   IconId: 73, Color: "#8a51b8", Seq: 2)
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
                (Name: "Residential", Description: "Residential Address",  IconId: 14, Color: "#d0a225", Seq: 1),
                (Name: "Municipal",   Description: "Municipal Location",   IconId: 25, Color: "#8d3434", Seq: 2),
                (Name: "Community",   Description: "Community Location",   IconId: 2,  Color: "#56d7a6", Seq: 3),
                (Name: "Road",        Description: "Road",                 IconId: 51, Color: "#267d5c", Seq: 4)
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
            if (!context.RateTypes.Any(x => x.name == "Standard" && x.tenantGuid == tg))
            {
                context.RateTypes.Add(new RateType
                {
                    name = "Standard",
                    description = "Standard Rate",
                    color = "#1cb02d",
                    sequence = 1,
                    tenantGuid = tg,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                });
                created++;
            }
            context.SaveChanges();
            Console.WriteLine($"  ✓ Rate Types ({created} records)");


            // ──────────────────────────────────────────────
            // Tax Codes
            // ──────────────────────────────────────────────
            created = 0;
            var taxCodes = new[]
            {
                (Name: "HST",    Description: "Harmonized Sales Tax (15%)",  Code: "HST",    Rate: 15.0m, IsDefault: true,  IsExempt: false, Seq: 1),
                (Name: "Exempt", Description: "Tax exempt",                  Code: "EXEMPT", Rate: 0m,    IsDefault: false, IsExempt: true,  Seq: 2)
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
            // Charge Types  (depends on Currency being set up)
            // ──────────────────────────────────────────────
            created = 0;
            var chargeTypes = new[]
            {
                (Name: "Hall Rental",      Description: "Recreation centre hall rental fee",  Color: "#7C3AED", IsRevenue: true,  IsTaxable: false, Seq: 1),
                (Name: "Equipment Rental", Description: "Equipment rental fee",               Color: "#2563EB", IsRevenue: true,  IsTaxable: false, Seq: 2)
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
            // Event Types  (link first ChargeType created above for rental-style events)
            // ──────────────────────────────────────────────
            created = 0;
            int? hallRentalChargeTypeId = context.ChargeTypes
                .Where(x => x.name == "Hall Rental" && x.tenantGuid == tg)
                .Select(x => (int?)x.id)
                .FirstOrDefault();

            var eventTypes = new[]
            {
                (Name: "Hall Rental",      Desc: "General facility rental booking",  Color: "#818cf8", ReqAgreement: true,  ReqPayment: true,  ReqDeposit: true,  IsInternal: false, Price: 150.00m, Seq: 1),
                (Name: "Community Event",  Desc: "Committee-organized community event", Color: "#34d399", ReqAgreement: false, ReqPayment: false, ReqDeposit: false, IsInternal: true,  Price: 0m,      Seq: 2),
                (Name: "Council Meeting",  Desc: "Town council meeting",             Color: "#3632f5", ReqAgreement: false, ReqPayment: false, ReqDeposit: false, IsInternal: true,  Price: 0m,      Seq: 3),
                (Name: "Fundraiser",       Desc: "Fundraising event or bingo night", Color: "#fbbf24", ReqAgreement: false, ReqPayment: false, ReqDeposit: false, IsInternal: true,  Price: 0m,      Seq: 4)
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
                        requiresRentalAgreement = et.ReqAgreement,
                        requiresExternalContact = et.ReqAgreement,
                        requiresPayment = et.ReqPayment,
                        requiresDeposit = et.ReqDeposit,
                        requiresBarService = false,
                        allowsTicketSales = false,
                        isInternalEvent = et.IsInternal,
                        defaultPrice = et.Price > 0 ? et.Price : (decimal?)null,
                        chargeTypeId = et.ReqPayment ? hallRentalChargeTypeId : null,
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
                (Name: "Operations",    Description: "Town Operations Crew",   IconId: 28, Color: "#2889f0"),
                (Name: "Council",       Description: "Town Council",           IconId: 3,  Color: "#3632f5"),
                (Name: "Rec Committee", Description: "Recreation Committee",   IconId: 46, Color: "#f63131")
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
                (Name: "Rental Agreement",       Description: "Signed rental agreement / contract",       Color: "#7C3AED", Seq: 1),
                (Name: "Insurance Certificate",  Description: "Liability insurance certificate",          Color: "#2563EB", Seq: 2),
                (Name: "Permit",                 Description: "Required permits (liquor, fire, etc.)",    Color: "#D97706", Seq: 3),
                (Name: "Receipt",                Description: "Payment receipt or proof of payment",      Color: "#059669", Seq: 4),
                (Name: "Other",                  Description: "Miscellaneous supporting documents",       Color: "#6B7280", Seq: 5)
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
                    objectGuid = tg,        // match tenant guid for consistency
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
