using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Foundation.Scheduler.Database;
using Foundation.Scheduler.Services;


namespace Scheduler.Tests
{
    /// <summary>
    /// Base class for financial service tests.
    ///
    /// Creates a fresh SQLite in-memory database for each test class instance,
    /// seeds the minimal reference data that FinancialManagementService requires,
    /// and provides factory methods for creating test scenarios.
    ///
    /// SQLite is used instead of EF UseInMemoryDatabase because the service
    /// calls Database.BeginTransaction(), which the in-memory provider does not support.
    /// </summary>
    public class FinancialTestFixture : IDisposable
    {
        // ── Well-known test tenant ──
        public static readonly Guid TenantGuid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        public const int TestUserId = 1;

        // ── Seeded entity IDs (set during SeedReferenceData) ──
        public int DraftStatusId { get; private set; }
        public int PartiallyPaidStatusId { get; private set; }
        public int PaidStatusId { get; private set; }
        public int VoidedStatusId { get; private set; }

        public int PendingChargeStatusId { get; private set; }
        public int InvoicedChargeStatusId { get; private set; }
        public int PaidChargeStatusId { get; private set; }

        public int CurrencyId { get; private set; }
        public int ReceiptTypeId { get; private set; }
        public int FiscalPeriodId { get; private set; }
        public int ChargeTypeId { get; private set; }
        public int RevenueAccountTypeId { get; private set; }

        // ── Infrastructure ──
        private readonly SqliteConnection _connection;
        protected SchedulerContext Context { get; private set; }
        protected FinancialManagementService Service { get; private set; }


        public FinancialTestFixture()
        {
            //
            // SQLite in-memory: the connection must stay OPEN for the lifetime
            // of the fixture — closing it drops the database.
            //
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<SchedulerContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new TestSchedulerContext(options);
            Context.Database.EnsureCreated();

            //
            // Disable foreign key enforcement — the tests focus on business logic
            // correctness, not referential integrity.  Without this, every test
            // would need to seed the full entity graph (Client → ClientType →
            // TimeZone → Country → StateProvince, etc.) just to create an Invoice.
            //
            Context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");

            SeedReferenceData();

            var logger = NullLogger<FinancialManagementService>.Instance;
            Service = new FinancialManagementService(Context, logger);
        }


        /// <summary>
        /// Seeds the minimal reference data required by FinancialManagementService.
        /// </summary>
        private void SeedReferenceData()
        {
            // ── Account Types ──
            var revenueAccountType = new AccountType
            {
                name = "Revenue",
                description = "Revenue accounts",
                isRevenue = true,
                active = true,
                deleted = false,
                objectGuid = Guid.NewGuid()
            };
            Context.AccountTypes.Add(revenueAccountType);
            Context.SaveChanges();
            RevenueAccountTypeId = revenueAccountType.id;

            // ── Invoice Statuses ──
            var draft = new InvoiceStatus { name = "Draft", description = "Draft", sequence = 1, active = true, deleted = false, objectGuid = Guid.NewGuid() };
            var partiallyPaid = new InvoiceStatus { name = "Partially Paid", description = "Partially paid", sequence = 2, active = true, deleted = false, objectGuid = Guid.NewGuid() };
            var paid = new InvoiceStatus { name = "Paid", description = "Paid in full", sequence = 3, active = true, deleted = false, objectGuid = Guid.NewGuid() };
            var voided = new InvoiceStatus { name = "Voided", description = "Voided", sequence = 4, active = true, deleted = false, objectGuid = Guid.NewGuid() };
            Context.InvoiceStatuses.AddRange(draft, partiallyPaid, paid, voided);
            Context.SaveChanges();

            DraftStatusId = draft.id;
            PartiallyPaidStatusId = partiallyPaid.id;
            PaidStatusId = paid.id;
            VoidedStatusId = voided.id;

            // ── Charge Statuses ──
            var pending = new ChargeStatus { name = "Pending", description = "Pending", sequence = 1, active = true, deleted = false, objectGuid = Guid.NewGuid() };
            var invoiced = new ChargeStatus { name = "Invoiced", description = "Invoiced", sequence = 2, active = true, deleted = false, objectGuid = Guid.NewGuid() };
            var paidCharge = new ChargeStatus { name = "Paid", description = "Paid", sequence = 3, active = true, deleted = false, objectGuid = Guid.NewGuid() };
            Context.ChargeStatuses.AddRange(pending, invoiced, paidCharge);
            Context.SaveChanges();

            PendingChargeStatusId = pending.id;
            InvoicedChargeStatusId = invoiced.id;
            PaidChargeStatusId = paidCharge.id;

            // ── Currency ── (seed early: ChargeType has a required FK to Currency)
            var cad = new Currency { name = "Canadian Dollar", code = "CAD", description = "CAD", sequence = 1, active = true, deleted = false, objectGuid = Guid.NewGuid() };
            Context.Currencies.Add(cad);
            Context.SaveChanges();
            CurrencyId = cad.id;

            // ── Charge Types ──
            var chargeType = new ChargeType
            {
                tenantGuid = TenantGuid,
                name = "Standard",
                description = "Standard charge",
                currencyId = CurrencyId,
                sequence = 1,
                active = true,
                deleted = false,
                objectGuid = Guid.NewGuid(),
                versionNumber = 1
            };
            Context.ChargeTypes.Add(chargeType);
            Context.SaveChanges();
            ChargeTypeId = chargeType.id;

            // ── Financial Categories ── (pre-seed to avoid GetOrCreateFinancialCategoryAsync
            // creating one with missing required fields under SQLite)
            Context.FinancialCategories.Add(new FinancialCategory
            {
                tenantGuid = TenantGuid,
                name = "Booking Revenue",
                description = "Revenue from event bookings",
                code = "REV-BOOKING",
                accountTypeId = RevenueAccountTypeId,
                active = true,
                deleted = false,
                objectGuid = Guid.NewGuid(),
                versionNumber = 1
            });
            // Also seed a Cash account so GL posting works
            Context.FinancialCategories.Add(new FinancialCategory
            {
                tenantGuid = TenantGuid,
                name = "Cash",
                description = "Cash/bank account for GL contra entries",
                code = "CASH",
                accountTypeId = RevenueAccountTypeId,
                active = true,
                deleted = false,
                objectGuid = Guid.NewGuid(),
                versionNumber = 1
            });
            Context.SaveChanges();

            // ── Period Statuses ──
            var openPeriodStatus = new PeriodStatus { name = "Open", description = "Open period", sequence = 1, active = true, deleted = false, objectGuid = Guid.NewGuid() };
            Context.PeriodStatuses.Add(openPeriodStatus);
            Context.SaveChanges();

            // ── Tenant Profile ──
            Context.TenantProfiles.Add(new TenantProfile
            {
                tenantGuid = TenantGuid,
                name = "Test Tenant",
                invoiceNumberMask = "INV-{YYYY}-{NNNN}",
                receiptNumberMask = "REC-{YYYY}-{NNNN}",
                active = true,
                deleted = false,
                objectGuid = Guid.NewGuid(),
                versionNumber = 1
            });
            Context.SaveChanges();

            // ── Receipt Type ──
            var receiptType = new ReceiptType
            {
                tenantGuid = TenantGuid,
                name = "Payment Receipt",
                description = "Standard receipt",
                sequence = 1,
                active = true,
                deleted = false,
                objectGuid = Guid.NewGuid()
            };
            Context.ReceiptTypes.Add(receiptType);
            Context.SaveChanges();
            ReceiptTypeId = receiptType.id;

            // ── Fiscal Period (open, covering current year) ──
            var fp = new FiscalPeriod
            {
                tenantGuid = TenantGuid,
                name = "FY " + DateTime.UtcNow.Year,
                description = "Full fiscal year " + DateTime.UtcNow.Year,
                periodType = "Annual",
                fiscalYear = DateTime.UtcNow.Year,
                periodNumber = 1,
                startDate = new DateTime(DateTime.UtcNow.Year, 1, 1),
                endDate = new DateTime(DateTime.UtcNow.Year, 12, 31),
                periodStatusId = openPeriodStatus.id,
                active = true,
                deleted = false,
                objectGuid = Guid.NewGuid(),
                versionNumber = 1
            };
            Context.FiscalPeriods.Add(fp);
            Context.SaveChanges();
            FiscalPeriodId = fp.id;
        }


        // ────────────────────────────────────────────────────────────────────
        //  Test data factory methods
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a ScheduledEvent with the given number of EventCharges.
        /// Returns the event ID.
        /// </summary>
        public int SeedEventWithCharges(int chargeCount = 2, decimal unitPrice = 100m, decimal quantity = 1m, decimal taxRate = 0.15m)
        {
            // Ensure an EventStatus exists (required FK)
            var eventStatus = Context.EventStatuses.FirstOrDefault();
            if (eventStatus == null)
            {
                eventStatus = new EventStatus { name = "Confirmed", description = "Confirmed", sequence = 1, active = true, deleted = false, objectGuid = Guid.NewGuid() };
                Context.EventStatuses.Add(eventStatus);
                Context.SaveChanges();
            }

            var scheduledEvent = new ScheduledEvent
            {
                tenantGuid = TenantGuid,
                name = "Test Event " + Guid.NewGuid().ToString("N").Substring(0, 8),
                startDateTime = DateTime.UtcNow.AddDays(7),
                endDateTime = DateTime.UtcNow.AddDays(7).AddHours(2),
                eventStatusId = eventStatus.id,
                active = true,
                deleted = false,
                objectGuid = Guid.NewGuid(),
                versionNumber = 1
            };

            Context.ScheduledEvents.Add(scheduledEvent);
            Context.SaveChanges();

            for (int i = 0; i < chargeCount; i++)
            {
                decimal extendedAmount = quantity * unitPrice;
                decimal taxAmount = extendedAmount * taxRate;
                decimal totalAmount = extendedAmount + taxAmount;

                Context.EventCharges.Add(new EventCharge
                {
                    tenantGuid = TenantGuid,
                    scheduledEventId = scheduledEvent.id,
                    chargeTypeId = ChargeTypeId,
                    chargeStatusId = PendingChargeStatusId,
                    currencyId = CurrencyId,
                    quantity = quantity,
                    unitPrice = unitPrice,
                    extendedAmount = extendedAmount,
                    taxAmount = taxAmount,
                    totalAmount = totalAmount,
                    description = $"Charge {i + 1}",
                    active = true,
                    deleted = false,
                    objectGuid = Guid.NewGuid(),
                    versionNumber = 1
                });
            }

            Context.SaveChanges();
            return scheduledEvent.id;
        }


        /// <summary>
        /// Creates an invoice from an event (the full happy-path flow).
        /// Returns the result from FinancialManagementService.
        /// </summary>
        public async Task<FinancialManagementService.FinancialOperationResult> CreateTestInvoiceAsync(int eventId)
        {
            return await Service.CreateInvoiceFromEventAsync(TenantGuid, eventId, TestUserId);
        }


        public void Dispose()
        {
            Context?.Dispose();
            _connection?.Dispose();
        }
    }
}
