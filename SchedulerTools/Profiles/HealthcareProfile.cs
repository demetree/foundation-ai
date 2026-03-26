using Foundation.Scheduler.Database;
using System;
using System.Linq;

namespace Foundation.Scheduler.CodeGeneration.Profiles
{
    /// <summary>
    /// Default configuration profile for healthcare clinics and home-care scheduling.
    /// Models appointments, practitioner roles, patient flow, and home-visit coordination.
    /// </summary>
    internal class HealthcareProfile : ITenantConfigurationProfile
    {
        public string Name => "Healthcare Clinic / HomeCare Scheduling";
        public string Description => "Clinic appointments, home visits, practitioner roles, and patient scheduling";

        public void Apply(SchedulerContext context, TenantConfigurationContext config)
        {
            Guid tg = config.TenantGuid;
            int created = 0;

            // ──────────────────────────────────────────────
            // Priorities
            // ──────────────────────────────────────────────
            var priorities = new[]
            {
                (Name: "Urgent",  Description: "Urgent — immediate attention required", Color: "#EF4444", Seq: 1),
                (Name: "High",    Description: "High Priority",                        Color: "#F59E0B", Seq: 2),
                (Name: "Normal",  Description: "Normal Priority",                      Color: "#3B82F6", Seq: 3),
                (Name: "Low",     Description: "Low Priority",                         Color: "#98948F", Seq: 4)
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
                (Name: "Clinic Appointments", Description: "In-clinic appointment schedule",   Color: "#3B82F6", IconId: 56, IsDefault: true),
                (Name: "Home Visits",         Description: "Home-care visit schedule",         Color: "#10B981", IconId: 14, IsDefault: false),
                (Name: "On-Call",             Description: "On-call and emergency coverage",   Color: "#EF4444", IconId: 46, IsDefault: false)
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
                (Name: "Physician",      Description: "Physician / Doctor",           IconId: 3,  Color: "#1D4ED8", IsBillable: true,  Seq: 1),
                (Name: "Nurse",          Description: "Registered Nurse",             IconId: 4,  Color: "#059669", IsBillable: true,  Seq: 2),
                (Name: "Home Care Aide", Description: "Home Care Aide / PSW",         IconId: 10, Color: "#7C3AED", IsBillable: true,  Seq: 3),
                (Name: "Support Staff",  Description: "Administrative / Support",     IconId: 28, Color: "#6B7280", IsBillable: false, Seq: 4),
                (Name: "Vehicle",        Description: "Company Vehicle",              IconId: 8,  Color: "#2a8479", IsBillable: false, Seq: 5)
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
                (Name: "Lead Practitioner", Description: "Lead Practitioner",   IconId: 3,  Color: "#1D4ED8", Seq: 1),
                (Name: "Attending",         Description: "Attending Clinician", IconId: 4,  Color: "#059669", Seq: 2),
                (Name: "Support",           Description: "Support Staff",      IconId: 28, Color: "#6B7280", Seq: 3)
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
                (Name: "Patient",            Description: "Patient",                        IconId: 2,  Color: "#3B82F6", Seq: 1),
                (Name: "Referral Source",     Description: "Referring physician / facility", IconId: 3,  Color: "#7C3AED", Seq: 2),
                (Name: "Insurance Provider",  Description: "Insurance or funding body",      IconId: 56, Color: "#059669", Seq: 3)
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
                (Name: "Clinic",                  Description: "Clinic or medical office",       IconId: 56, Color: "#3B82F6", Seq: 1),
                (Name: "Patient Home",            Description: "Patient's home address",         IconId: 14, Color: "#10B981", Seq: 2),
                (Name: "Long-Term Care Facility", Description: "Long-term care / nursing home",  IconId: 25, Color: "#7C3AED", Seq: 3),
                (Name: "Hospital",                Description: "Hospital",                       IconId: 46, Color: "#EF4444", Seq: 4)
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
                (Name: "Standard",    Description: "Standard billing rate",     Color: "#3B82F6", Seq: 1),
                (Name: "After-Hours", Description: "After-hours billing rate",  Color: "#F59E0B", Seq: 2),
                (Name: "Emergency",   Description: "Emergency billing rate",    Color: "#EF4444", Seq: 3)
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
                (Name: "HST",             Description: "Harmonized Sales Tax (15%)", Code: "HST",    Rate: 15.0m, IsDefault: true,  IsExempt: false, Seq: 1),
                (Name: "Exempt (Medical)", Description: "Tax exempt — medical",      Code: "EXEMPT", Rate: 0m,    IsDefault: false, IsExempt: true,  Seq: 2)
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
                (Name: "Consultation Fee",  Description: "In-clinic consultation fee",   Color: "#3B82F6", IsRevenue: true, IsTaxable: false, Seq: 1),
                (Name: "Home Visit Fee",    Description: "Home-care visit fee",          Color: "#10B981", IsRevenue: true, IsTaxable: false, Seq: 2),
                (Name: "Procedure Fee",     Description: "Medical procedure / lab fee",  Color: "#7C3AED", IsRevenue: true, IsTaxable: false, Seq: 3)
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
            int? consultChargeId = context.ChargeTypes
                .Where(x => x.name == "Consultation Fee" && x.tenantGuid == tg)
                .Select(x => (int?)x.id)
                .FirstOrDefault();

            int? homeVisitChargeId = context.ChargeTypes
                .Where(x => x.name == "Home Visit Fee" && x.tenantGuid == tg)
                .Select(x => (int?)x.id)
                .FirstOrDefault();

            int? procedureChargeId = context.ChargeTypes
                .Where(x => x.name == "Procedure Fee" && x.tenantGuid == tg)
                .Select(x => (int?)x.id)
                .FirstOrDefault();

            var eventTypes = new[]
            {
                (Name: "Initial Consultation", Desc: "New patient initial consultation",         Color: "#3B82F6", ReqPayment: true,  IsInternal: false, Price: 0m,   ChargeId: consultChargeId,  Seq: 1),
                (Name: "Follow-Up",            Desc: "Follow-up appointment",                   Color: "#6366F1", ReqPayment: true,  IsInternal: false, Price: 0m,   ChargeId: consultChargeId,  Seq: 2),
                (Name: "Home Visit",           Desc: "Home-care visit",                         Color: "#10B981", ReqPayment: true,  IsInternal: false, Price: 0m,   ChargeId: homeVisitChargeId, Seq: 3),
                (Name: "Therapy Session",      Desc: "Therapy or rehabilitation session",       Color: "#7C3AED", ReqPayment: true,  IsInternal: false, Price: 0m,   ChargeId: procedureChargeId, Seq: 4),
                (Name: "Lab / Diagnostic",     Desc: "Lab work or diagnostic procedure",        Color: "#F59E0B", ReqPayment: true,  IsInternal: false, Price: 0m,   ChargeId: procedureChargeId, Seq: 5)
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
                        defaultPrice = et.Price > 0 ? et.Price : (decimal?)null,
                        chargeTypeId = et.ChargeId,
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
                (Name: "Morning Clinic",   Description: "Morning clinic team",    IconId: 56, Color: "#3B82F6"),
                (Name: "Afternoon Clinic",  Description: "Afternoon clinic team",  IconId: 56, Color: "#6366F1"),
                (Name: "Home Care Team",   Description: "Home care visit team",   IconId: 14, Color: "#10B981")
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
                (Name: "Referral Letter",          Description: "Physician referral letter",                Color: "#3B82F6", Seq: 1),
                (Name: "Insurance Authorization",  Description: "Insurance pre-authorization document",     Color: "#059669", Seq: 2),
                (Name: "Health Record",            Description: "Patient health record or chart",           Color: "#7C3AED", Seq: 3),
                (Name: "Consent Form",             Description: "Patient consent or intake form",           Color: "#D97706", Seq: 4),
                (Name: "Notes",                    Description: "Clinical or visit notes",                  Color: "#6B7280", Seq: 5)
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
