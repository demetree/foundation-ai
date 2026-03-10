// AI-Developed — This file was significantly developed with AI assistance.
using System;
using System.Globalization;

using Foundation.Imaging.Pdf;


namespace Scheduler.Server.Services
{
    /// <summary>
    ///
    /// Generates receipt PDF documents using Foundation.Imaging SimplePdfDocument.
    ///
    /// Creates a receipt layout including:
    ///   - Tenant header (company name, address, contact info)
    ///   - Receipt number and date
    ///   - Payer details
    ///   - Amount received and payment method
    ///   - Description and notes
    ///
    /// Returns the raw PDF bytes for storage in the Document table.
    ///
    /// </summary>
    public class ReceiptPdfService
    {
        // ── Page dimensions (A4 in points) ──
        private const double PAGE_WIDTH = 595.28;
        private const double PAGE_HEIGHT = 841.89;

        // ── Margins ──
        private const double MARGIN_LEFT = 50;
        private const double MARGIN_RIGHT = 50;
        private const double MARGIN_TOP = 50;
        private const double CONTENT_WIDTH = PAGE_WIDTH - MARGIN_LEFT - MARGIN_RIGHT;

        // ── Colours (RGB) ──
        private const byte HEADER_R = 33, HEADER_G = 37, HEADER_B = 41;
        private const byte ACCENT_R = 76, ACCENT_G = 175, ACCENT_B = 80;        // Green accent (#4CAF50)
        private const byte TEXT_R = 50, TEXT_G = 50, TEXT_B = 50;
        private const byte LIGHT_R = 120, LIGHT_G = 120, LIGHT_B = 120;
        private const byte LINE_R = 200, LINE_G = 200, LINE_B = 200;

        // ── Font sizes ──
        private const double TITLE_SIZE = 24;
        private const double HEADING_SIZE = 11;
        private const double BODY_SIZE = 9;
        private const double SMALL_SIZE = 8;
        private const double AMOUNT_SIZE = 28;


        /// <summary>
        ///
        /// Generates a PDF receipt document and returns the raw bytes.
        ///
        /// </summary>
        public byte[] GenerateReceiptPdf(ReceiptPdfData data)
        {
            using (var doc = new SimplePdfDocument($"Receipt {data.ReceiptNumber}", data.TenantName))
            {
                var page = doc.AddPage(PAGE_WIDTH, PAGE_HEIGHT);
                double y = MARGIN_TOP;

                //
                // ── Header band ──
                //
                page.FillRect(0, 0, PAGE_WIDTH, 80, HEADER_R, HEADER_G, HEADER_B);
                page.DrawText("RECEIPT", SimplePdfFont.Bold, TITLE_SIZE,
                    MARGIN_LEFT, 35, 255, 255, 255);

                page.DrawText(data.TenantName ?? "", SimplePdfFont.Bold, HEADING_SIZE,
                    PAGE_WIDTH - MARGIN_RIGHT - page.MeasureText(data.TenantName ?? "", SimplePdfFont.Bold, HEADING_SIZE),
                    30, 255, 255, 255);

                string tenantAddr = BuildAddress(data.TenantAddress1, data.TenantCity);
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

                y = 110;

                //
                // ── Receipt details ──
                //
                y = DrawLabelValue(page, "Receipt #:", data.ReceiptNumber, MARGIN_LEFT, y);
                y = DrawLabelValue(page, "Date:", FormatDate(data.ReceiptDate), MARGIN_LEFT, y);

                if (!string.IsNullOrEmpty(data.PaymentMethod))
                {
                    y = DrawLabelValue(page, "Payment Method:", data.PaymentMethod, MARGIN_LEFT, y);
                }

                y += 10;

                //
                // ── Accent line ──
                //
                page.DrawLine(MARGIN_LEFT, y, PAGE_WIDTH - MARGIN_RIGHT, y, ACCENT_R, ACCENT_G, ACCENT_B, 2);
                y += 20;

                //
                // ── Received from ──
                //
                if (!string.IsNullOrEmpty(data.ClientName) || !string.IsNullOrEmpty(data.ContactName))
                {
                    page.DrawText("RECEIVED FROM", SimplePdfFont.Bold, BODY_SIZE, MARGIN_LEFT, y, ACCENT_R, ACCENT_G, ACCENT_B);
                    y += 16;

                    if (!string.IsNullOrEmpty(data.ClientName))
                    {
                        page.DrawText(data.ClientName, SimplePdfFont.Bold, BODY_SIZE, MARGIN_LEFT, y, TEXT_R, TEXT_G, TEXT_B);
                        y += 14;
                    }

                    if (!string.IsNullOrEmpty(data.ContactName))
                    {
                        page.DrawText(data.ContactName, SimplePdfFont.Regular, BODY_SIZE, MARGIN_LEFT, y, TEXT_R, TEXT_G, TEXT_B);
                        y += 14;
                    }

                    y += 10;
                }

                //
                // ── Amount block ──
                //
                page.FillRoundedRect(MARGIN_LEFT, y, CONTENT_WIDTH, 70, 8,
                    245, 245, 245);

                page.DrawText("AMOUNT RECEIVED", SimplePdfFont.Bold, BODY_SIZE,
                    MARGIN_LEFT + 20, y + 22, LIGHT_R, LIGHT_G, LIGHT_B);

                string amountStr = FormatMoney(data.Amount);
                page.DrawText(amountStr, SimplePdfFont.Bold, AMOUNT_SIZE,
                    MARGIN_LEFT + 20, y + 52, ACCENT_R, ACCENT_G, ACCENT_B);

                y += 90;

                //
                // ── Description ──
                //
                if (!string.IsNullOrEmpty(data.Description))
                {
                    page.DrawText("FOR", SimplePdfFont.Bold, BODY_SIZE, MARGIN_LEFT, y, ACCENT_R, ACCENT_G, ACCENT_B);
                    y += 16;
                    page.DrawText(data.Description, SimplePdfFont.Regular, BODY_SIZE, MARGIN_LEFT, y, TEXT_R, TEXT_G, TEXT_B);
                    y += 20;
                }

                //
                // ── Invoice reference ──
                //
                if (!string.IsNullOrEmpty(data.InvoiceNumber))
                {
                    y = DrawLabelValue(page, "Invoice Reference:", data.InvoiceNumber, MARGIN_LEFT, y);
                }

                //
                // ── Notes ──
                //
                if (!string.IsNullOrEmpty(data.Notes))
                {
                    y += 10;
                    page.DrawText("Notes:", SimplePdfFont.Bold, BODY_SIZE, MARGIN_LEFT, y, TEXT_R, TEXT_G, TEXT_B);
                    y += 14;
                    page.DrawText(data.Notes, SimplePdfFont.Regular, SMALL_SIZE, MARGIN_LEFT, y, LIGHT_R, LIGHT_G, LIGHT_B);
                }

                //
                // ── Footer ──
                //
                page.DrawLine(MARGIN_LEFT, PAGE_HEIGHT - 50, PAGE_WIDTH - MARGIN_RIGHT, PAGE_HEIGHT - 50, LINE_R, LINE_G, LINE_B);
                page.DrawTextCentered("Thank you for your payment",
                    SimplePdfFont.Regular, SMALL_SIZE,
                    MARGIN_LEFT, PAGE_HEIGHT - 38, CONTENT_WIDTH, LIGHT_R, LIGHT_G, LIGHT_B);

                return doc.Save();
            }
        }


        // ── Private helpers ──

        private static double DrawLabelValue(SimplePdfPage page, string label, string value, double x, double y)
        {
            page.DrawText(label, SimplePdfFont.Bold, BODY_SIZE, x, y, TEXT_R, TEXT_G, TEXT_B);
            double labelWidth = page.MeasureText(label, SimplePdfFont.Bold, BODY_SIZE);
            page.DrawText(value ?? "", SimplePdfFont.Regular, BODY_SIZE, x + labelWidth + 6, y, TEXT_R, TEXT_G, TEXT_B);
            return y + 16;
        }


        private static string BuildAddress(string address1, string city)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrEmpty(address1)) { parts.Add(address1); }
            if (!string.IsNullOrEmpty(city)) { parts.Add(city); }
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
    }


    /// <summary>
    /// Input data for receipt PDF generation.
    /// Populated by the controller from the Receipt + TenantProfile entities.
    /// </summary>
    public class ReceiptPdfData
    {
        // Tenant header
        public string TenantName { get; set; }
        public string TenantAddress1 { get; set; }
        public string TenantCity { get; set; }
        public string TenantPhone { get; set; }

        // Receipt details
        public string ReceiptNumber { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public string PaymentMethod { get; set; }

        // Payer
        public string ClientName { get; set; }
        public string ContactName { get; set; }

        // Amount
        public decimal Amount { get; set; }

        // Context
        public string Description { get; set; }
        public string InvoiceNumber { get; set; }
        public string Notes { get; set; }
    }
}
