using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;


namespace Scheduler.Tests.Financial
{
    /// <summary>
    /// Tests for FinancialManagementService.IssueRefundAsync.
    /// </summary>
    public class RefundTests : FinancialTestFixture
    {
        /// <summary>
        /// Helper: seeds event, creates invoice, records full payment.
        /// Returns (invoiceId, totalAmount).
        /// </summary>
        private async Task<(int invoiceId, decimal totalAmount)> SetupPaidInvoiceAsync(decimal unitPrice = 100m)
        {
            int eventId = SeedEventWithCharges(chargeCount: 1, unitPrice: unitPrice, quantity: 1m, taxRate: 0m);
            var invoiceResult = await CreateTestInvoiceAsync(eventId);
            Assert.True(invoiceResult.Success);
            int invoiceId = (int)invoiceResult.Data["invoiceId"];

            var invoice = await Context.Invoices.FirstAsync(i => i.id == invoiceId);
            decimal total = invoice.totalAmount;

            // Pay in full
            var payResult = await Service.RecordInvoicePaymentAsync(
                TenantGuid, invoiceId, total, TestUserId);
            Assert.True(payResult.Success, payResult.ErrorMessage);

            return (invoiceId, total);
        }


        [Fact]
        public async Task IssueRefund_PartialRefund_UpdatesBalance()
        {
            var (invoiceId, total) = await SetupPaidInvoiceAsync(200m);

            // Act: refund $80 of $200
            var refundResult = await Service.IssueRefundAsync(
                TenantGuid, invoiceId, 80m, TestUserId, "Partial refund test");

            // Assert
            Assert.True(refundResult.Success, refundResult.ErrorMessage);

            var invoice = await Context.Invoices.FirstAsync(i => i.id == invoiceId);
            Assert.Equal(120m, invoice.amountPaid);  // 200 - 80

            // Assert: invoice status should cascade back to Partially Paid
            Assert.Equal(PartiallyPaidStatusId, invoice.invoiceStatusId);
        }


        [Fact]
        public async Task IssueRefund_FullRefund_ZeroesBalance()
        {
            var (invoiceId, total) = await SetupPaidInvoiceAsync(150m);

            // Act: refund the entire amount
            var refundResult = await Service.IssueRefundAsync(
                TenantGuid, invoiceId, 150m, TestUserId, "Full refund");

            Assert.True(refundResult.Success, refundResult.ErrorMessage);

            var invoice = await Context.Invoices.FirstAsync(i => i.id == invoiceId);
            Assert.Equal(0m, invoice.amountPaid);

            // Assert: invoice status should cascade back to Draft
            Assert.Equal(DraftStatusId, invoice.invoiceStatusId);
        }


        [Fact]
        public async Task IssueRefund_ExceedsAmountPaid_IsRejected()
        {
            var (invoiceId, total) = await SetupPaidInvoiceAsync(100m);

            // Act: try to refund more than was paid
            var refundResult = await Service.IssueRefundAsync(
                TenantGuid, invoiceId, total + 50m, TestUserId, "Over-refund");

            Assert.False(refundResult.Success);
            Assert.Contains("exceeds", refundResult.ErrorMessage);
        }


        [Fact]
        public async Task IssueRefund_CreatesNegativeReceipt()
        {
            var (invoiceId, _) = await SetupPaidInvoiceAsync(100m);

            var refundResult = await Service.IssueRefundAsync(
                TenantGuid, invoiceId, 40m, TestUserId, "Refund receipt test");
            Assert.True(refundResult.Success, refundResult.ErrorMessage);

            // Assert: a negative-amount receipt exists
            var refundReceipt = await Context.Receipts
                .Where(r => r.invoiceId == invoiceId && r.amount < 0)
                .FirstOrDefaultAsync();
            Assert.NotNull(refundReceipt);
            Assert.Equal(-40m, refundReceipt.amount);
            Assert.Contains("REFUND", refundReceipt.description);
        }
    }
}
