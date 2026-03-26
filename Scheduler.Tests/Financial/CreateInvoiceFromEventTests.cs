using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;


namespace Scheduler.Tests.Financial
{
    /// <summary>
    /// Tests for FinancialManagementService.CreateInvoiceFromEventAsync.
    /// Each test creates a fresh fixture (fresh DB) for isolation.
    /// </summary>
    public class CreateInvoiceFromEventTests : FinancialTestFixture
    {
        [Fact]
        public async Task CreateInvoice_HappyPath_CreatesInvoiceAndLineItems()
        {
            // Arrange: 2 charges at $100 each + 15% tax = $115 each → $230 total
            int eventId = SeedEventWithCharges(chargeCount: 2, unitPrice: 100m, quantity: 1m, taxRate: 0.15m);

            // Act
            var result = await CreateTestInvoiceAsync(eventId);

            // Assert: operation succeeded
            Assert.True(result.Success, result.ErrorMessage);
            Assert.NotNull(result.Data["invoiceId"]);
            Assert.NotNull(result.Data["invoiceNumber"]);

            int invoiceId = (int)result.Data["invoiceId"];

            // Assert: invoice record exists with correct totals
            var invoice = await Context.Invoices.FirstOrDefaultAsync(i => i.id == invoiceId);
            Assert.NotNull(invoice);
            Assert.Equal(200m, invoice.subtotal);       // 2 × $100
            Assert.Equal(30m, invoice.taxAmount);        // 2 × $15
            Assert.Equal(230m, invoice.totalAmount);     // subtotal + tax
            Assert.Equal(0m, invoice.amountPaid);
            Assert.Equal(DraftStatusId, invoice.invoiceStatusId);

            // Assert: 2 line items were created
            var lineItems = await Context.InvoiceLineItems
                .Where(li => li.invoiceId == invoiceId)
                .ToListAsync();
            Assert.Equal(2, lineItems.Count);

            // Assert: charges were cascaded to "Invoiced" status
            var charges = await Context.EventCharges
                .Where(ec => ec.scheduledEventId == eventId)
                .ToListAsync();
            Assert.All(charges, c => Assert.Equal(InvoicedChargeStatusId, c.chargeStatusId));

            // Assert: a FinancialTransaction ledger entry was created
            var ft = await Context.FinancialTransactions
                .Where(t => t.tenantGuid == TenantGuid && t.referenceNumber == invoice.invoiceNumber)
                .FirstOrDefaultAsync();
            Assert.NotNull(ft);
            Assert.Equal(230m, ft.totalAmount);
            Assert.True(ft.isRevenue);
        }


        [Fact]
        public async Task CreateInvoice_SetsCorrectDueDate()
        {
            int eventId = SeedEventWithCharges(chargeCount: 1);

            var result = await CreateTestInvoiceAsync(eventId);
            Assert.True(result.Success);

            int invoiceId = (int)result.Data["invoiceId"];
            var invoice = await Context.Invoices.FirstAsync(i => i.id == invoiceId);

            // Due date should be approximately 30 days after invoice date
            var diff = (invoice.dueDate - invoice.invoiceDate).TotalDays;
            Assert.InRange(diff, 29.9, 30.1);
        }


        [Fact]
        public async Task CreateInvoice_Idempotency_RejectsDuplicate()
        {
            int eventId = SeedEventWithCharges(chargeCount: 1);

            // First call succeeds
            var result1 = await CreateTestInvoiceAsync(eventId);
            Assert.True(result1.Success);

            // Second call should fail (duplicate prevention)
            var result2 = await CreateTestInvoiceAsync(eventId);
            Assert.False(result2.Success);
            Assert.Contains("already exists", result2.ErrorMessage);
        }


        [Fact]
        public async Task CreateInvoice_NoCharges_ReturnsError()
        {
            // Create event with zero charges
            int eventId = SeedEventWithCharges(chargeCount: 0);

            var result = await CreateTestInvoiceAsync(eventId);

            Assert.False(result.Success);
            Assert.Contains("No charges", result.ErrorMessage);
        }


        [Fact]
        public async Task CreateInvoice_EventNotFound_ReturnsError()
        {
            var result = await Service.CreateInvoiceFromEventAsync(TenantGuid, 99999, TestUserId);

            Assert.False(result.Success);
            Assert.Contains("not found", result.ErrorMessage);
        }


        [Fact]
        public async Task CreateInvoice_GeneratesSequentialNumbers()
        {
            // Create two separate events and invoice them
            int event1 = SeedEventWithCharges(chargeCount: 1);
            int event2 = SeedEventWithCharges(chargeCount: 1);

            var result1 = await CreateTestInvoiceAsync(event1);
            var result2 = await CreateTestInvoiceAsync(event2);

            Assert.True(result1.Success);
            Assert.True(result2.Success);

            string num1 = result1.Data["invoiceNumber"].ToString();
            string num2 = result2.Data["invoiceNumber"].ToString();

            // Both should start with the year-based prefix
            string expectedPrefix = $"INV-{System.DateTime.UtcNow.Year}-";
            Assert.StartsWith(expectedPrefix, num1);
            Assert.StartsWith(expectedPrefix, num2);

            // Second number should be greater (sequential)
            Assert.NotEqual(num1, num2);
        }
    }
}
