using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;


namespace Scheduler.Tests.Financial
{
    /// <summary>
    /// Tests for FinancialManagementService.VoidInvoiceAsync.
    /// </summary>
    public class VoidInvoiceTests : FinancialTestFixture
    {
        [Fact]
        public async Task VoidInvoice_SetsStatusToVoided()
        {
            // Arrange
            int eventId = SeedEventWithCharges(chargeCount: 1, unitPrice: 100m, quantity: 1m, taxRate: 0m);
            var invoiceResult = await CreateTestInvoiceAsync(eventId);
            Assert.True(invoiceResult.Success);
            int invoiceId = (int)invoiceResult.Data["invoiceId"];

            // Act
            var voidResult = await Service.VoidInvoiceAsync(
                TenantGuid, invoiceId, TestUserId, "Testing void");

            // Assert
            Assert.True(voidResult.Success, voidResult.ErrorMessage);

            var invoice = await Context.Invoices.FirstAsync(i => i.id == invoiceId);
            Assert.Equal(VoidedStatusId, invoice.invoiceStatusId);
            Assert.Contains("VOIDED", invoice.notes);
        }


        [Fact]
        public async Task VoidInvoice_ResetsChargeStatusToPending()
        {
            int eventId = SeedEventWithCharges(chargeCount: 2, unitPrice: 50m, quantity: 1m, taxRate: 0m);
            var invoiceResult = await CreateTestInvoiceAsync(eventId);
            Assert.True(invoiceResult.Success);
            int invoiceId = (int)invoiceResult.Data["invoiceId"];

            // Confirm charges are "Invoiced" after invoice creation
            var chargesBefore = await Context.EventCharges
                .Where(ec => ec.scheduledEventId == eventId).ToListAsync();
            Assert.All(chargesBefore, c => Assert.Equal(InvoicedChargeStatusId, c.chargeStatusId));

            // Act: void the invoice
            var voidResult = await Service.VoidInvoiceAsync(
                TenantGuid, invoiceId, TestUserId, "Testing");
            Assert.True(voidResult.Success, voidResult.ErrorMessage);

            // Assert: charges reset to "Pending"
            var chargesAfter = await Context.EventCharges
                .Where(ec => ec.scheduledEventId == eventId).ToListAsync();
            Assert.All(chargesAfter, c => Assert.Equal(PendingChargeStatusId, c.chargeStatusId));
        }


        [Fact]
        public async Task VoidInvoice_CreatesReversingTransaction()
        {
            int eventId = SeedEventWithCharges(chargeCount: 1, unitPrice: 200m, quantity: 1m, taxRate: 0m);
            var invoiceResult = await CreateTestInvoiceAsync(eventId);
            Assert.True(invoiceResult.Success);
            int invoiceId = (int)invoiceResult.Data["invoiceId"];
            string invoiceNumber = invoiceResult.Data["invoiceNumber"].ToString();

            // Act
            var voidResult = await Service.VoidInvoiceAsync(
                TenantGuid, invoiceId, TestUserId, "Duplicate");
            Assert.True(voidResult.Success, voidResult.ErrorMessage);

            // Assert: a reversing FinancialTransaction with negative amount exists
            var reversing = await Context.FinancialTransactions
                .Where(t => t.tenantGuid == TenantGuid && t.totalAmount < 0 && t.referenceNumber == invoiceNumber)
                .FirstOrDefaultAsync();
            Assert.NotNull(reversing);
            Assert.Equal(-200m, reversing.totalAmount);
        }


        [Fact]
        public async Task VoidInvoice_AllowsReInvoicing()
        {
            int eventId = SeedEventWithCharges(chargeCount: 1, unitPrice: 100m, quantity: 1m, taxRate: 0m);

            // Create + void
            var result1 = await CreateTestInvoiceAsync(eventId);
            Assert.True(result1.Success);
            int invoiceId = (int)result1.Data["invoiceId"];

            var voidResult = await Service.VoidInvoiceAsync(TenantGuid, invoiceId, TestUserId, "Re-invoice");
            Assert.True(voidResult.Success, voidResult.ErrorMessage);

            // Act: create a new invoice for the same event
            var result2 = await CreateTestInvoiceAsync(eventId);

            // Assert: the second invoice should succeed
            Assert.True(result2.Success, result2.ErrorMessage);
            Assert.NotEqual(result1.Data["invoiceId"], result2.Data["invoiceId"]);
        }


        [Fact]
        public async Task VoidInvoice_WithPayments_IsRejected()
        {
            int eventId = SeedEventWithCharges(chargeCount: 1, unitPrice: 100m, quantity: 1m, taxRate: 0m);
            var invoiceResult = await CreateTestInvoiceAsync(eventId);
            Assert.True(invoiceResult.Success);
            int invoiceId = (int)invoiceResult.Data["invoiceId"];

            // Record a payment so the invoice has amountPaid > 0
            var payResult = await Service.RecordInvoicePaymentAsync(
                TenantGuid, invoiceId, 50m, TestUserId);
            Assert.True(payResult.Success, payResult.ErrorMessage);

            // Act: try to void — should be rejected
            var voidResult = await Service.VoidInvoiceAsync(
                TenantGuid, invoiceId, TestUserId, "Should fail");

            Assert.False(voidResult.Success);
            Assert.Contains("payments", voidResult.ErrorMessage);
        }
    }
}
