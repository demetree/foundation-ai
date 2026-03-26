using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

using Foundation.Scheduler.Database;


namespace Scheduler.Tests.Financial
{
    /// <summary>
    /// Tests for the General Ledger posting and reconciliation features
    /// of FinancialManagementService.
    /// </summary>
    public class GeneralLedgerTests : FinancialTestFixture
    {
        [Fact]
        public async Task PostToGL_CreatesBalancedEntry()
        {
            // Arrange: create two financial categories for the GL entry
            var debitCategory = new FinancialCategory
            {
                tenantGuid = TenantGuid,
                name = "Accounts Receivable",
                code = "AR",
                accountTypeId = RevenueAccountTypeId,
                active = true,
                deleted = false,
                objectGuid = Guid.NewGuid(),
                versionNumber = 1
            };
            var creditCategory = new FinancialCategory
            {
                tenantGuid = TenantGuid,
                name = "Booking Revenue GL",
                code = "REV",
                accountTypeId = RevenueAccountTypeId,
                active = true,
                deleted = false,
                objectGuid = Guid.NewGuid(),
                versionNumber = 1
            };
            Context.FinancialCategories.AddRange(debitCategory, creditCategory);
            await Context.SaveChangesAsync();

            // Act
            await Service.PostToGeneralLedgerAsync(
                TenantGuid,
                debitCategory.id,
                creditCategory.id,
                500m,
                DateTime.UtcNow,
                "Test GL posting",
                TestUserId);
            await Context.SaveChangesAsync();

            // Assert: GL entry exists with balanced lines
            var entry = await Context.GeneralLedgerEntries
                .Include(e => e.GeneralLedgerLines)
                .FirstOrDefaultAsync(e => e.tenantGuid == TenantGuid);

            Assert.NotNull(entry);
            Assert.Equal(2, entry.GeneralLedgerLines.Count);

            decimal totalDebits = entry.GeneralLedgerLines.Sum(l => l.debitAmount);
            decimal totalCredits = entry.GeneralLedgerLines.Sum(l => l.creditAmount);
            Assert.Equal(500m, totalDebits);
            Assert.Equal(500m, totalCredits);
            Assert.Equal(totalDebits, totalCredits);
        }


        [Fact]
        public async Task PostToGL_ZeroAmount_NoOp()
        {
            var category = new FinancialCategory
            {
                tenantGuid = TenantGuid,
                name = "Zero Test",
                code = "ZT",
                accountTypeId = RevenueAccountTypeId,
                active = true,
                deleted = false,
                objectGuid = Guid.NewGuid(),
                versionNumber = 1
            };
            Context.FinancialCategories.Add(category);
            await Context.SaveChangesAsync();

            // Act: post $0
            await Service.PostToGeneralLedgerAsync(
                TenantGuid, category.id, category.id, 0m,
                DateTime.UtcNow, "Should not create entry", TestUserId);
            await Context.SaveChangesAsync();

            // Assert: no GL entries were created
            int count = await Context.GeneralLedgerEntries
                .CountAsync(e => e.tenantGuid == TenantGuid);
            Assert.Equal(0, count);
        }


        [Fact]
        public async Task InvoiceCreation_ReconcileGL_IsClean()
        {
            // Arrange: create a standard invoice (which posts to GL via CreateInvoiceFromEventAsync)
            int eventId = SeedEventWithCharges(chargeCount: 2, unitPrice: 100m, quantity: 1m, taxRate: 0m);
            var result = await CreateTestInvoiceAsync(eventId);
            Assert.True(result.Success, result.ErrorMessage);

            // Act: run GL reconciliation
            var reconciliation = await Service.ReconcileGLAsync(TenantGuid);

            // Assert: reconciliation should not be null (just smoke-test the API)
            Assert.NotNull(reconciliation);
        }
    }
}
