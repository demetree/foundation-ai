using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;


namespace Scheduler.Tests.Financial
{
    /// <summary>
    /// Tests for FinancialManagementService.RecordInvoicePaymentAsync.
    /// </summary>
    public class InvoicePaymentTests : FinancialTestFixture
    {
        [Fact]
        public async Task RecordPayment_FullPayment_UpdatesAmountPaid()
        {
            // Arrange: create invoice for $230 (2 charges × $100 + 15% tax)
            int eventId = SeedEventWithCharges(chargeCount: 2, unitPrice: 100m, quantity: 1m, taxRate: 0.15m);
            var invoiceResult = await CreateTestInvoiceAsync(eventId);
            Assert.True(invoiceResult.Success);
            int invoiceId = (int)invoiceResult.Data["invoiceId"];

            // Act: pay the full $230
            var payResult = await Service.RecordInvoicePaymentAsync(
                TenantGuid, invoiceId, 230m, TestUserId);

            // Assert
            Assert.True(payResult.Success, payResult.ErrorMessage);
            Assert.Equal(230m, payResult.Data["invoiceAmountPaid"]);
            Assert.Equal(0m, payResult.Data["invoiceBalance"]);

            // Assert: invoice status cascaded to Paid
            var invoice = await Context.Invoices.FirstAsync(i => i.id == invoiceId);
            Assert.Equal(PaidStatusId, invoice.invoiceStatusId);

            // Assert: receipt was created
            var receipt = await Context.Receipts
                .FirstOrDefaultAsync(r => r.invoiceId == invoiceId && r.amount == 230m);
            Assert.NotNull(receipt);

            // Assert: event charges cascaded to "Paid" status
            var charges = await Context.EventCharges
                .Where(ec => ec.scheduledEventId == eventId)
                .ToListAsync();
            Assert.All(charges, c => Assert.Equal(PaidChargeStatusId, c.chargeStatusId));
        }


        [Fact]
        public async Task RecordPayment_PartialPayment_CalculatesBalance()
        {
            int eventId = SeedEventWithCharges(chargeCount: 1, unitPrice: 200m, quantity: 1m, taxRate: 0m);
            var invoiceResult = await CreateTestInvoiceAsync(eventId);
            Assert.True(invoiceResult.Success);
            int invoiceId = (int)invoiceResult.Data["invoiceId"];

            // Act: pay $80 of $200
            var payResult = await Service.RecordInvoicePaymentAsync(
                TenantGuid, invoiceId, 80m, TestUserId);

            // Assert
            Assert.True(payResult.Success, payResult.ErrorMessage);
            Assert.Equal(80m, payResult.Data["invoiceAmountPaid"]);
            Assert.Equal(120m, payResult.Data["invoiceBalance"]);

            // Assert: invoice status should be Partially Paid
            var invoice = await Context.Invoices.FirstAsync(i => i.id == invoiceId);
            Assert.Equal(PartiallyPaidStatusId, invoice.invoiceStatusId);
        }


        [Fact]
        public async Task RecordPayment_Overpayment_IsRejected()
        {
            int eventId = SeedEventWithCharges(chargeCount: 1, unitPrice: 100m, quantity: 1m, taxRate: 0m);
            var invoiceResult = await CreateTestInvoiceAsync(eventId);
            Assert.True(invoiceResult.Success);
            int invoiceId = (int)invoiceResult.Data["invoiceId"];

            // Act: try to pay $150 on a $100 invoice
            var payResult = await Service.RecordInvoicePaymentAsync(
                TenantGuid, invoiceId, 150m, TestUserId);

            // Assert: should be rejected
            Assert.False(payResult.Success);
            Assert.Contains("exceeds", payResult.ErrorMessage);
        }


        [Fact]
        public async Task RecordPayment_CreatesReceipt()
        {
            int eventId = SeedEventWithCharges(chargeCount: 1, unitPrice: 50m, quantity: 1m, taxRate: 0m);
            var invoiceResult = await CreateTestInvoiceAsync(eventId);
            Assert.True(invoiceResult.Success);
            int invoiceId = (int)invoiceResult.Data["invoiceId"];

            // Act
            var payResult = await Service.RecordInvoicePaymentAsync(
                TenantGuid, invoiceId, 50m, TestUserId);

            Assert.True(payResult.Success, payResult.ErrorMessage);

            // Assert: a receipt was created with the correct receipt number format
            string receiptNumber = payResult.Data["receiptNumber"].ToString();
            Assert.StartsWith($"REC-{System.DateTime.UtcNow.Year}-", receiptNumber);

            var receipt = await Context.Receipts
                .FirstOrDefaultAsync(r => r.receiptNumber == receiptNumber);
            Assert.NotNull(receipt);
            Assert.Equal(50m, receipt.amount);
            Assert.Equal(invoiceId, receipt.invoiceId);
        }
    }
}
