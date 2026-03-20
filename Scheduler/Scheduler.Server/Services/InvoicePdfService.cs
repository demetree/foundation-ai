// AI-Developed — This file was significantly developed with AI assistance.
using System;
using System.Globalization;

using Foundation.Imaging.Pdf;


namespace Scheduler.Server.Services
{
    /// <summary>
    ///
    /// Generates invoice PDF documents using Foundation.Imaging SimplePdfDocument.
    ///
    /// Creates a professional invoice layout including:
    ///   - Tenant header (company name, address, contact info)
    ///   - Invoice number, date, and due date
    ///   - Client billing details
    ///   - Line item table (description, qty, unit price, amount)
    ///   - Subtotal, tax, and total
    ///   - Notes/payment terms
    ///
    /// Returns the raw PDF bytes for storage in the Document table.
    ///
    /// </summary>
    public class InvoicePdfService
    {
        // ── Page dimensions (A4 in points: 72 pts = 1 inch) ──
        private const double PAGE_WIDTH = 595.28;
        private const double PAGE_HEIGHT = 841.89;

        // ── Margins ──
        private const double MARGIN_LEFT = 50;
        private const double MARGIN_RIGHT = 50;
        private const double MARGIN_TOP = 50;
        private const double CONTENT_WIDTH = PAGE_WIDTH - MARGIN_LEFT - MARGIN_RIGHT;

        // ── Colours (RGB) ──
        private const byte HEADER_R = 33, HEADER_G = 37, HEADER_B = 41;         // Dark header
        private const byte ACCENT_R = 33, ACCENT_G = 150, ACCENT_B = 243;       // Blue accent (#2196F3)
        private const byte TEXT_R = 50, TEXT_G = 50, TEXT_B = 50;                // Body text
        private const byte LIGHT_R = 120, LIGHT_G = 120, LIGHT_B = 120;         // Secondary text
        private const byte TABLE_HEADER_R = 240, TABLE_HEADER_G = 240, TABLE_HEADER_B = 240; // Row bg
        private const byte LINE_R = 200, LINE_G = 200, LINE_B = 200;            // Lines

        // ── Font sizes ──
        private const double TITLE_SIZE = 24;
        private const double HEADING_SIZE = 11;
        private const double BODY_SIZE = 9;
        private const double SMALL_SIZE = 8;


        /// <summary>
        ///
        /// Generates a PDF invoice document and returns the raw bytes.
        ///
        /// </summary>
        public byte[] GenerateInvoicePdf(InvoicePdfData data)
        {
            SimplePdfDocument doc = new SimplePdfDocument($"Invoice {data.InvoiceNumber}", data.TenantName);
            
            var page = doc.AddPage(PAGE_WIDTH, PAGE_HEIGHT);
            double y = MARGIN_TOP;

            //
            // ── Header band ──
            //
            page.FillRect(0, 0, PAGE_WIDTH, 80, HEADER_R, HEADER_G, HEADER_B);
            page.DrawText("INVOICE", SimplePdfFont.Bold, TITLE_SIZE,
                MARGIN_LEFT, 35, 255, 255, 255);

            page.DrawText(data.TenantName ?? "", SimplePdfFont.Bold, HEADING_SIZE,
                PAGE_WIDTH - MARGIN_RIGHT - page.MeasureText(data.TenantName ?? "", SimplePdfFont.Bold, HEADING_SIZE),
                30, 255, 255, 255);

            string tenantAddr = BuildTenantAddress(data);
            if (tenantAddr.Length > 0)
            {
                page.DrawText(tenantAddr, SimplePdfFont.Regular, SMALL_SIZE,
                    PAGE_WIDTH - MARGIN_RIGHT - page.MeasureText(tenantAddr, SimplePdfFont.Regular, SMALL_SIZE),
                    48, 200, 200, 200);
            }

            if (!string.IsNullOrEmpty(data.TenantPhone))
            {
                page.DrawText(data.TenantPhone, SimplePdfFont.Regular, SMALL_SIZE,
                    PAGE_WIDTH - MARGIN_RIGHT - page.MeasureText(data.TenantPhone, SimplePdfFont.Regular, SMALL_SIZE),
                    60, 200, 200, 200);
            }

            y = 100;

            //
            // ── Invoice details (left) + Bill To (right) ──
            //
            y = DrawLabelValue(page, "Invoice #:", data.InvoiceNumber, MARGIN_LEFT, y);
            y = DrawLabelValue(page, "Date:", FormatDate(data.InvoiceDate), MARGIN_LEFT, y);
            y = DrawLabelValue(page, "Due Date:", FormatDate(data.DueDate), MARGIN_LEFT, y);

            if (!string.IsNullOrEmpty(data.Status))
            {
                y = DrawLabelValue(page, "Status:", data.Status, MARGIN_LEFT, y);
            }

            // Bill To (right side)
            double rightX = PAGE_WIDTH / 2 + 20;
            double billToY = 100;

            page.DrawText("BILL TO", SimplePdfFont.Bold, BODY_SIZE, rightX, billToY, ACCENT_R, ACCENT_G, ACCENT_B);
            billToY += 16;

            if (!string.IsNullOrEmpty(data.ClientName))
            {
                page.DrawText(data.ClientName, SimplePdfFont.Bold, BODY_SIZE, rightX, billToY, TEXT_R, TEXT_G, TEXT_B);
                billToY += 14;
            }

            if (!string.IsNullOrEmpty(data.ContactName))
            {
                page.DrawText(data.ContactName, SimplePdfFont.Regular, BODY_SIZE, rightX, billToY, TEXT_R, TEXT_G, TEXT_B);
                billToY += 14;
            }

            y = Math.Max(y, billToY) + 10;

            //
            // ── Accent line ──
            //
            page.DrawLine(MARGIN_LEFT, y, PAGE_WIDTH - MARGIN_RIGHT, y, ACCENT_R, ACCENT_G, ACCENT_B, 2);
            y += 15;

            //
            // ── Line item table ──
            //
            double colDesc = MARGIN_LEFT;
            double colQty = MARGIN_LEFT + CONTENT_WIDTH * 0.50;
            double colPrice = MARGIN_LEFT + CONTENT_WIDTH * 0.62;
            double colTax = MARGIN_LEFT + CONTENT_WIDTH * 0.76;
            double colTotal = MARGIN_LEFT + CONTENT_WIDTH * 0.88;

            // Table header
            page.FillRect(MARGIN_LEFT, y - 2, CONTENT_WIDTH, 16,
                TABLE_HEADER_R, TABLE_HEADER_G, TABLE_HEADER_B);

            page.DrawText("Description", SimplePdfFont.Bold, SMALL_SIZE, colDesc + 4, y + 10, TEXT_R, TEXT_G, TEXT_B);
            page.DrawText("Qty", SimplePdfFont.Bold, SMALL_SIZE, colQty, y + 10, TEXT_R, TEXT_G, TEXT_B);
            page.DrawText("Unit Price", SimplePdfFont.Bold, SMALL_SIZE, colPrice, y + 10, TEXT_R, TEXT_G, TEXT_B);
            page.DrawText("Tax", SimplePdfFont.Bold, SMALL_SIZE, colTax, y + 10, TEXT_R, TEXT_G, TEXT_B);
            page.DrawText("Total", SimplePdfFont.Bold, SMALL_SIZE, colTotal, y + 10, TEXT_R, TEXT_G, TEXT_B);
            y += 18;

            // Line items
            if (data.LineItems != null)
            {
                bool altRow = false;
                foreach (var item in data.LineItems)
                {
                    if (altRow)
                    {
                        page.FillRect(MARGIN_LEFT, y - 2, CONTENT_WIDTH, 16, 248, 248, 248);
                    }

                    string desc = item.Description ?? "";
                    if (desc.Length > 60) { desc = desc.Substring(0, 57) + "..."; }

                    page.DrawText(desc, SimplePdfFont.Regular, SMALL_SIZE, colDesc + 4, y + 10, TEXT_R, TEXT_G, TEXT_B);
                    page.DrawText(FormatNumber(item.Quantity), SimplePdfFont.Regular, SMALL_SIZE, colQty, y + 10, TEXT_R, TEXT_G, TEXT_B);
                    page.DrawText(FormatMoney(item.UnitPrice), SimplePdfFont.Regular, SMALL_SIZE, colPrice, y + 10, TEXT_R, TEXT_G, TEXT_B);
                    page.DrawText(FormatMoney(item.TaxAmount), SimplePdfFont.Regular, SMALL_SIZE, colTax, y + 10, TEXT_R, TEXT_G, TEXT_B);
                    page.DrawText(FormatMoney(item.TotalAmount), SimplePdfFont.Regular, SMALL_SIZE, colTotal, y + 10, TEXT_R, TEXT_G, TEXT_B);
                    y += 16;
                    altRow = !altRow;
                }
            }

            // Table bottom line
            page.DrawLine(MARGIN_LEFT, y + 2, PAGE_WIDTH - MARGIN_RIGHT, y + 2, LINE_R, LINE_G, LINE_B);
            y += 15;

            //
            // ── Totals ──
            //
            double totalsX = colTax - 30;
            y = DrawTotalLine(page, "Subtotal:", FormatMoney(data.Subtotal), totalsX, colTotal, y);
            y = DrawTotalLine(page, "Tax:", FormatMoney(data.TaxAmount), totalsX, colTotal, y);

            page.DrawLine(totalsX, y, PAGE_WIDTH - MARGIN_RIGHT, y, ACCENT_R, ACCENT_G, ACCENT_B, 1);
            y += 4;

            page.DrawText("TOTAL:", SimplePdfFont.Bold, HEADING_SIZE, totalsX, y + 12, TEXT_R, TEXT_G, TEXT_B);
            page.DrawText(FormatMoney(data.TotalAmount), SimplePdfFont.Bold, HEADING_SIZE, colTotal, y + 12, ACCENT_R, ACCENT_G, ACCENT_B);
            y += 20;

            if (data.AmountPaid > 0)
            {
                y = DrawTotalLine(page, "Amount Paid:", FormatMoney(data.AmountPaid), totalsX, colTotal, y);
                y = DrawTotalLine(page, "Balance Due:", FormatMoney(data.TotalAmount - data.AmountPaid), totalsX, colTotal, y);
            }

            //
            // ── Notes ──
            //
            if (!string.IsNullOrEmpty(data.Notes))
            {
                y += 20;
                page.DrawText("Notes:", SimplePdfFont.Bold, BODY_SIZE, MARGIN_LEFT, y, TEXT_R, TEXT_G, TEXT_B);
                y += 14;
                page.DrawText(data.Notes, SimplePdfFont.Regular, SMALL_SIZE, MARGIN_LEFT, y, LIGHT_R, LIGHT_G, LIGHT_B);
            }

            //
            // ── Footer ──
            //
            page.DrawLine(MARGIN_LEFT, PAGE_HEIGHT - 50, PAGE_WIDTH - MARGIN_RIGHT, PAGE_HEIGHT - 50, LINE_R, LINE_G, LINE_B);
            page.DrawTextCentered("Thank you for your business",
                SimplePdfFont.Regular, SMALL_SIZE,
                MARGIN_LEFT, PAGE_HEIGHT - 38, CONTENT_WIDTH, LIGHT_R, LIGHT_G, LIGHT_B);

            return doc.Save();
            
        }


        // ── Private helpers ──

        private static double DrawLabelValue(SimplePdfPage page, string label, string value, double x, double y)
        {
            page.DrawText(label, SimplePdfFont.Bold, BODY_SIZE, x, y, TEXT_R, TEXT_G, TEXT_B);
            double labelWidth = page.MeasureText(label, SimplePdfFont.Bold, BODY_SIZE);
            page.DrawText(value ?? "", SimplePdfFont.Regular, BODY_SIZE, x + labelWidth + 6, y, TEXT_R, TEXT_G, TEXT_B);
            return y + 16;
        }


        private static double DrawTotalLine(SimplePdfPage page, string label, string value, double labelX, double valueX, double y)
        {
            page.DrawText(label, SimplePdfFont.Regular, BODY_SIZE, labelX, y + 12, TEXT_R, TEXT_G, TEXT_B);
            page.DrawText(value, SimplePdfFont.Regular, BODY_SIZE, valueX, y + 12, TEXT_R, TEXT_G, TEXT_B);
            return y + 16;
        }


        private static string BuildTenantAddress(InvoicePdfData data)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrEmpty(data.TenantAddress1)) { parts.Add(data.TenantAddress1); }
            if (!string.IsNullOrEmpty(data.TenantCity)) { parts.Add(data.TenantCity); }
            return string.Join(", ", parts);
        }


        private static string FormatDate(DateTime? dt)
        {
            return dt?.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture) ?? "";
        }

        private static string FormatMoney(decimal value)
        {
            return "$" + value.ToString("N2", CultureInfo.InvariantCulture);
        }

        private static string FormatNumber(decimal value)
        {
            if (value == Math.Floor(value))
            {
                return ((int)value).ToString(CultureInfo.InvariantCulture);
            }

            return value.ToString("N2", CultureInfo.InvariantCulture);
        }
    }


    /// <summary>
    /// Input data for invoice PDF generation.
    /// Populated by the controller from the Invoice + InvoiceLineItem + TenantProfile entities.
    /// </summary>
    public class InvoicePdfData
    {
        // Tenant header
        public string TenantName { get; set; }
        public string TenantAddress1 { get; set; }
        public string TenantCity { get; set; }
        public string TenantPhone { get; set; }

        // Invoice details
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }

        // Client / contact
        public string ClientName { get; set; }
        public string ContactName { get; set; }

        // Totals
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }

        // Notes
        public string Notes { get; set; }

        // Line items
        public InvoiceLineItemPdfData[] LineItems { get; set; }
    }


    /// <summary>
    /// Single line item for the invoice PDF.
    /// </summary>
    public class InvoiceLineItemPdfData
    {
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
