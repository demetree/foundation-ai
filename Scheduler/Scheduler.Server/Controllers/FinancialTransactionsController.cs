using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.Controllers;
using static Foundation.Auditor.AuditEngine;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// Custom partial class extension of the auto-generated FinancialTransactionsController.
    ///
    /// Adds endpoints beyond basic CRUD:
    ///   - Dashboard summary aggregation
    ///   - Category-level breakdown for reporting
    ///   - Formatted Excel financial report export
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    /// </summary>
    public partial class FinancialTransactionsController
    {
        /// <summary>
        /// Returns an aggregated summary of financial transactions for the dashboard.
        /// 
        /// Computes totals, counts, and monthly breakdown entirely on the database,
        /// eliminating the need to download all transactions to the client for aggregation.
        /// 
        /// Filters by office and year. Active and non-deleted only.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/FinancialTransactions/Summary")]
        public async Task<IActionResult> GetFinancialTransactionSummary(
            int? financialOfficeId = null,
            int? year = null,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            //
            // Security check — reader role required.
            //
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "Attempt was made to read a financial summary by a user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            //
            // Base query — tenant scoped, active, non-deleted.
            //
            IQueryable<Database.FinancialTransaction> query = _context.FinancialTransactions
                .Where(ft => ft.tenantGuid == userTenantGuid && ft.active == true && ft.deleted == false);

            if (financialOfficeId.HasValue)
            {
                query = query.Where(ft => ft.financialOfficeId == financialOfficeId.Value);
            }

            int filterYear = year ?? DateTime.UtcNow.Year;

            //
            // Totals — revenue and expense.
            //
            decimal totalRevenue = await query
                .Where(ft => ft.isRevenue == true)
                .SumAsync(ft => ft.totalAmount, cancellationToken);

            decimal totalExpense = await query
                .Where(ft => ft.isRevenue == false)
                .SumAsync(ft => ft.totalAmount, cancellationToken);

            int transactionCount = await query.CountAsync(cancellationToken);

            //
            // Monthly breakdown for the given year — grouped by month.
            //
            var monthlyBreakdown = await query
                .Where(ft => ft.transactionDate.Year == filterYear)
                .GroupBy(ft => new { ft.transactionDate.Month, ft.isRevenue })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    IsRevenue = g.Key.isRevenue,
                    Total = g.Sum(ft => ft.totalAmount)
                })
                .ToListAsync(cancellationToken);

            //
            // Build a 12-month array.
            //
            var months = new List<object>();
            for (int m = 1; m <= 12; m++)
            {
                decimal income = monthlyBreakdown
                    .Where(x => x.Month == m && x.IsRevenue == true)
                    .Sum(x => x.Total);

                decimal expense = monthlyBreakdown
                    .Where(x => x.Month == m && x.IsRevenue == false)
                    .Sum(x => x.Total);

                months.Add(new { month = m, income, expense });
            }

            await CreateAuditEventAsync(AuditType.ReadList,
                $"Financial transaction summary read. Year={filterYear}, OfficeId={financialOfficeId?.ToString() ?? "all"}, Count={transactionCount}");

            return Ok(new
            {
                totalRevenue,
                totalExpense,
                netIncome = totalRevenue - totalExpense,
                transactionCount,
                year = filterYear,
                monthlyBreakdown = months
            });
        }


        /// <summary>
        /// Returns category-level aggregation of financial transactions for the dashboard summary table.
        ///
        /// Groups transactions by FinancialCategory and returns totals/counts for each,
        /// split into revenue and expense sections. Used by the client to render the
        /// income-vs-expense breakdown table below the monthly chart.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/FinancialTransactions/CategoryBreakdown")]
        public async Task<IActionResult> GetCategoryBreakdown(
            int? financialOfficeId = null,
            int? year = null,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "CategoryBreakdown requested by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            IQueryable<Database.FinancialTransaction> query = _context.FinancialTransactions
                .Include(ft => ft.financialCategory)
                .Where(ft => ft.tenantGuid == userTenantGuid && ft.active == true && ft.deleted == false);

            if (financialOfficeId.HasValue)
            {
                query = query.Where(ft => ft.financialOfficeId == financialOfficeId.Value);
            }

            int filterYear = year ?? DateTime.UtcNow.Year;

            query = query.Where(ft => ft.transactionDate.Year == filterYear);

            var breakdown = await query
                .GroupBy(ft => new
                {
                    ft.financialCategoryId,
                    CategoryName = ft.financialCategory != null ? ft.financialCategory.name : "Uncategorized",
                    CategoryCode = ft.financialCategory != null ? ft.financialCategory.code : "",
                    ft.isRevenue
                })
                .Select(g => new
                {
                    categoryId = g.Key.financialCategoryId,
                    categoryName = g.Key.CategoryName,
                    code = g.Key.CategoryCode,
                    isRevenue = g.Key.isRevenue,
                    total = g.Sum(ft => ft.totalAmount),
                    count = g.Count()
                })
                .OrderBy(x => x.code)
                .ToListAsync(cancellationToken);

            await CreateAuditEventAsync(AuditType.ReadList,
                $"Category breakdown read. Year={filterYear}, Categories={breakdown.Count}");

            return Ok(new
            {
                year = filterYear,
                categories = breakdown
            });
        }


        /// <summary>
        /// Aggregates transactions by month for a given year, split by revenue/expense.
        /// Returns 12 entries (one per month) with income and expense totals.
        /// Used by the dashboard's monthly breakdown chart.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/FinancialTransactions/MonthlyBreakdown")]
        public async Task<IActionResult> GetMonthlyBreakdown(
            int? financialOfficeId = null,
            int? year = null,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "MonthlyBreakdown requested by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            IQueryable<Database.FinancialTransaction> query = _context.FinancialTransactions
                .Where(ft => ft.tenantGuid == userTenantGuid && ft.active == true && ft.deleted == false);

            if (financialOfficeId.HasValue)
            {
                query = query.Where(ft => ft.financialOfficeId == financialOfficeId.Value);
            }

            int filterYear = year ?? DateTime.UtcNow.Year;

            query = query.Where(ft => ft.transactionDate.Year == filterYear);

            var monthlyData = await query
                .GroupBy(ft => new { ft.transactionDate.Month, ft.isRevenue })
                .Select(g => new
                {
                    month = g.Key.Month,
                    isRevenue = g.Key.isRevenue,
                    total = g.Sum(ft => ft.totalAmount)
                })
                .ToListAsync(cancellationToken);

            // Build 12-month arrays
            var income = new decimal[12];
            var expenses = new decimal[12];
            foreach (var entry in monthlyData)
            {
                var idx = entry.month - 1; // Month is 1-based, array is 0-based
                if (entry.isRevenue)
                    income[idx] = entry.total;
                else
                    expenses[idx] = entry.total;
            }

            await CreateAuditEventAsync(AuditType.ReadList,
                $"Monthly breakdown read. Year={filterYear}");

            return Ok(new
            {
                year = filterYear,
                income,
                expenses
            });
        }


        /// <summary>
        /// Generates a formatted Excel financial report and returns it as a file download.
        ///
        /// Produces a workbook with three sheets:
        ///   - Summary: Category-level totals grouped by Revenue/Expense with grand totals
        ///   - Income: All revenue transactions for the year
        ///   - Expenses: All expense transactions for the year
        ///
        /// Designed for the PHMC recreation committee to produce council meeting reports.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.OnePerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/FinancialTransactions/ExportFinancialReport")]
        public async Task<IActionResult> ExportFinancialReport(
            int? financialOfficeId = null,
            int? year = null,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "ExportFinancialReport requested by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            int filterYear = year ?? DateTime.UtcNow.Year;

            //
            // Load tenant profile for the report header
            //
            var tenantProfile = await _context.TenantProfiles
                .Where(tp => tp.tenantGuid == userTenantGuid && tp.active == true && tp.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            string tenantName = tenantProfile?.name ?? "Financial Report";

            //
            // Load all transactions for the year with their categories
            //
            IQueryable<Database.FinancialTransaction> query = _context.FinancialTransactions
                .Include(ft => ft.financialCategory)
                .Where(ft => ft.tenantGuid == userTenantGuid
                    && ft.active == true && ft.deleted == false
                    && ft.transactionDate.Year == filterYear);

            if (financialOfficeId.HasValue)
            {
                query = query.Where(ft => ft.financialOfficeId == financialOfficeId.Value);
            }

            var transactions = await query
                .OrderBy(ft => ft.transactionDate)
                .ToListAsync(cancellationToken);


            //
            // Build the Excel workbook
            //
            using var workbook = new XLWorkbook();

            BuildSummarySheet(workbook, transactions, tenantName, filterYear);
            BuildTransactionSheet(workbook, transactions, "Income", true, filterYear);
            BuildTransactionSheet(workbook, transactions, "Expenses", false, filterYear);

            //
            // Write to memory and return
            //
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"{tenantName.Replace(" ", "_")}_Financial_Report_{filterYear}.xlsx";

            await CreateAuditEventAsync(AuditType.Miscellaneous,
                $"Financial report exported: Year={filterYear}, Transactions={transactions.Count}");

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }


        //
        // ── Excel Report Helpers ──
        //

        private static void BuildSummarySheet(
            XLWorkbook workbook,
            List<Database.FinancialTransaction> transactions,
            string tenantName,
            int year)
        {
            var ws = workbook.Worksheets.Add("Summary");

            //
            // Report header
            //
            ws.Cell("A1").Value = tenantName;
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 16;

            ws.Cell("A2").Value = $"Financial Summary — {year}";
            ws.Cell("A2").Style.Font.FontSize = 12;
            ws.Cell("A2").Style.Font.FontColor = XLColor.DimGray;

            ws.Cell("A3").Value = $"Generated {DateTime.UtcNow:MMMM dd, yyyy}";
            ws.Cell("A3").Style.Font.FontSize = 10;
            ws.Cell("A3").Style.Font.FontColor = XLColor.DimGray;

            int row = 5;

            //
            // Revenue section
            //
            ws.Cell(row, 1).Value = "REVENUE";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 13;
            row++;

            // Headers
            ws.Cell(row, 1).Value = "Code";
            ws.Cell(row, 2).Value = "Category";
            ws.Cell(row, 3).Value = "Transactions";
            ws.Cell(row, 4).Value = "Total";
            StyleHeaderRow(ws, row, 4);
            row++;

            var revenueByCategory = transactions
                .Where(t => t.isRevenue)
                .GroupBy(t => new
                {
                    Code = t.financialCategory?.code ?? "",
                    Name = t.financialCategory?.name ?? "Uncategorized"
                })
                .OrderBy(g => g.Key.Code)
                .ToList();

            decimal revenueGrandTotal = 0;
            int revenueGrandCount = 0;

            foreach (var group in revenueByCategory)
            {
                decimal total = group.Sum(t => t.totalAmount);
                int count = group.Count();

                ws.Cell(row, 1).Value = group.Key.Code;
                ws.Cell(row, 2).Value = group.Key.Name;
                ws.Cell(row, 3).Value = count;
                ws.Cell(row, 4).Value = total;
                ws.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
                row++;

                revenueGrandTotal += total;
                revenueGrandCount += count;
            }

            // Revenue total
            ws.Cell(row, 2).Value = "Total Revenue";
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 3).Value = revenueGrandCount;
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Cell(row, 4).Value = revenueGrandTotal;
            ws.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 4).Style.Font.FontColor = XLColor.DarkGreen;
            row += 2;

            //
            // Expense section
            //
            ws.Cell(row, 1).Value = "EXPENSES";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 13;
            row++;

            ws.Cell(row, 1).Value = "Code";
            ws.Cell(row, 2).Value = "Category";
            ws.Cell(row, 3).Value = "Transactions";
            ws.Cell(row, 4).Value = "Total";
            StyleHeaderRow(ws, row, 4);
            row++;

            var expenseByCategory = transactions
                .Where(t => !t.isRevenue)
                .GroupBy(t => new
                {
                    Code = t.financialCategory?.code ?? "",
                    Name = t.financialCategory?.name ?? "Uncategorized"
                })
                .OrderBy(g => g.Key.Code)
                .ToList();

            decimal expenseGrandTotal = 0;
            int expenseGrandCount = 0;

            foreach (var group in expenseByCategory)
            {
                decimal total = group.Sum(t => t.totalAmount);
                int count = group.Count();

                ws.Cell(row, 1).Value = group.Key.Code;
                ws.Cell(row, 2).Value = group.Key.Name;
                ws.Cell(row, 3).Value = count;
                ws.Cell(row, 4).Value = total;
                ws.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
                row++;

                expenseGrandTotal += total;
                expenseGrandCount += count;
            }

            // Expense total
            ws.Cell(row, 2).Value = "Total Expenses";
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 3).Value = expenseGrandCount;
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Cell(row, 4).Value = expenseGrandTotal;
            ws.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 4).Style.Font.FontColor = XLColor.DarkRed;
            row += 2;

            //
            // Net income
            //
            ws.Cell(row, 2).Value = "NET INCOME";
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.Font.FontSize = 13;
            ws.Cell(row, 4).Value = revenueGrandTotal - expenseGrandTotal;
            ws.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 4).Style.Font.FontSize = 13;
            ws.Cell(row, 4).Style.Font.FontColor = (revenueGrandTotal - expenseGrandTotal) >= 0
                ? XLColor.DarkGreen
                : XLColor.DarkRed;

            ws.Columns().AdjustToContents();
        }


        private static void BuildTransactionSheet(
            XLWorkbook workbook,
            List<Database.FinancialTransaction> allTransactions,
            string sheetName,
            bool isRevenue,
            int year)
        {
            var ws = workbook.Worksheets.Add(sheetName);
            var transactions = allTransactions.Where(t => t.isRevenue == isRevenue).ToList();

            //
            // Column headers
            //
            int row = 1;
            ws.Cell(row, 1).Value = "Date";
            ws.Cell(row, 2).Value = "Code";
            ws.Cell(row, 3).Value = "Category";
            ws.Cell(row, 4).Value = "Description";
            ws.Cell(row, 5).Value = "Amount";
            ws.Cell(row, 6).Value = "HST";
            ws.Cell(row, 7).Value = "Total";
            ws.Cell(row, 8).Value = "Reference #";
            ws.Cell(row, 9).Value = "Notes";
            StyleHeaderRow(ws, row, 9);
            row++;

            //
            // Data rows
            //
            decimal runningTotal = 0;

            foreach (var tx in transactions)
            {
                ws.Cell(row, 1).Value = tx.transactionDate;
                ws.Cell(row, 1).Style.DateFormat.Format = "yyyy-MM-dd";
                ws.Cell(row, 2).Value = tx.financialCategory?.code ?? "";
                ws.Cell(row, 3).Value = tx.financialCategory?.name ?? "";
                ws.Cell(row, 4).Value = tx.description ?? "";
                ws.Cell(row, 5).Value = tx.amount;
                ws.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
                ws.Cell(row, 6).Value = tx.taxAmount;
                ws.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
                ws.Cell(row, 7).Value = tx.totalAmount;
                ws.Cell(row, 7).Style.NumberFormat.Format = "$#,##0.00";
                ws.Cell(row, 8).Value = tx.referenceNumber ?? "";
                ws.Cell(row, 9).Value = tx.notes ?? "";

                runningTotal += tx.totalAmount;
                row++;
            }

            //
            // Totals row
            //
            ws.Cell(row, 4).Value = $"Total {sheetName}";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 5).Value = transactions.Sum(t => t.amount);
            ws.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(row, 5).Style.Font.Bold = true;
            ws.Cell(row, 6).Value = transactions.Sum(t => t.taxAmount);
            ws.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(row, 6).Style.Font.Bold = true;
            ws.Cell(row, 7).Value = runningTotal;
            ws.Cell(row, 7).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(row, 7).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();
        }


        private static void StyleHeaderRow(IXLWorksheet ws, int row, int columnCount)
        {
            for (int col = 1; col <= columnCount; col++)
            {
                ws.Cell(row, col).Style.Font.Bold = true;
                ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8E8E8");
                ws.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }
        }


        /// <summary>
        /// Returns a chronological timeline of all financial operations for a specific event.
        ///
        /// Merges charges and transactions into a unified timeline sorted by date.
        /// Each entry includes: date, type (charge/payment/deposit/refund/expense),
        /// description, amount, and the user who created the version if available.
        ///
        /// Designed for the PHMC rec committee to quickly reference when answering
        /// phone inquiries about event finances.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/FinancialTransactions/EventFinancialTimeline/{scheduledEventId}")]
        public async Task<IActionResult> GetEventFinancialTimeline(
            int scheduledEventId,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "EventFinancialTimeline requested by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            var timelineEntries = new List<object>();

            //
            // Load event charges
            //
            var charges = await _context.EventCharges
                .Include(c => c.chargeType)
                .Include(c => c.scheduledEvent)
                .Where(c => c.tenantGuid == userTenantGuid
                    && c.scheduledEventId == scheduledEventId
                    && c.active == true && c.deleted == false)
                .OrderBy(c => c.id)
                .ToListAsync(cancellationToken);

            foreach (var charge in charges)
            {
                string chargeLabel = charge.isDeposit ? "Deposit" : "Charge";

                //
                // Use the exported date as entry date, falling back to the event's start date
                //
                var chargeDate = charge.exportedDate
                    ?? charge.scheduledEvent?.startDateTime
                    ?? DateTime.UtcNow;

                timelineEntries.Add(new
                {
                    date = chargeDate,
                    type = chargeLabel.ToLowerInvariant(),
                    icon = charge.isDeposit ? "lock" : "receipt",
                    description = $"{chargeLabel}: {charge.chargeType?.name ?? "Unknown"} — {charge.description ?? charge.notes ?? ""}",
                    amount = charge.extendedAmount,
                    isPositive = true
                });

                //
                // If the deposit was refunded, add a refund entry
                //
                if (charge.isDeposit && charge.depositRefundedDate.HasValue)
                {
                    timelineEntries.Add(new
                    {
                        date = charge.depositRefundedDate.Value,
                        type = "refund",
                        icon = "arrow-counterclockwise",
                        description = $"Deposit Refunded: {charge.chargeType?.name ?? "Unknown"}",
                        amount = charge.extendedAmount,
                        isPositive = false
                    });
                }
            }

            //
            // Load financial transactions linked to this event
            //
            var transactions = await _context.FinancialTransactions
                .Include(t => t.financialCategory)
                .Where(t => t.tenantGuid == userTenantGuid
                    && t.scheduledEventId == scheduledEventId
                    && t.active == true && t.deleted == false)
                .OrderBy(t => t.transactionDate)
                .ToListAsync(cancellationToken);

            foreach (var tx in transactions)
            {
                string txType = tx.isRevenue ? "income" : "expense";
                string txIcon = tx.isRevenue ? "arrow-down-circle" : "arrow-up-circle";

                timelineEntries.Add(new
                {
                    date = tx.transactionDate,
                    type = txType,
                    icon = txIcon,
                    description = $"{(tx.isRevenue ? "Income" : "Expense")}: {tx.financialCategory?.name ?? ""} — {tx.description ?? ""}",
                    amount = tx.totalAmount,
                    isPositive = tx.isRevenue
                });
            }

            //
            // Sort chronologically
            //
            var sortedTimeline = timelineEntries
                .OrderBy(e => ((dynamic)e).date)
                .ToList();

            await CreateAuditEventAsync(AuditType.ReadList,
                $"Event financial timeline read. EventId={scheduledEventId}, Entries={sortedTimeline.Count}");

            return Ok(sortedTimeline);
        }


        /// <summary>
        /// Returns all outstanding (unreturned) deposits across the tenant.
        ///
        /// Includes the associated event name and charge type for display
        /// in the financial dashboard's outstanding deposits widget.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/FinancialTransactions/OutstandingDeposits")]
        public async Task<IActionResult> GetOutstandingDeposits(
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "OutstandingDeposits requested by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            var deposits = await _context.EventCharges
                .Include(c => c.chargeType)
                .Include(c => c.scheduledEvent)
                .Where(c => c.tenantGuid == userTenantGuid
                    && c.isDeposit == true
                    && c.depositRefundedDate == null
                    && c.active == true && c.deleted == false)
                .OrderBy(c => c.id)
                .Select(c => new
                {
                    chargeId = c.id,
                    eventId = c.scheduledEventId,
                    eventName = c.scheduledEvent != null ? c.scheduledEvent.name : "Unknown",
                    chargeType = c.chargeType != null ? c.chargeType.name : "Deposit",
                    amount = c.extendedAmount,
                    eventDate = c.scheduledEvent != null ? c.scheduledEvent.startDateTime : (DateTime?)null
                })
                .ToListAsync(cancellationToken);

            await CreateAuditEventAsync(AuditType.ReadList,
                $"Outstanding deposits read. Count={deposits.Count}");

            return Ok(new
            {
                count = deposits.Count,
                totalAmount = deposits.Sum(d => d.amount),
                deposits
            });
        }


        // ────────────────────────────────────────────────────────────────────────
        //  RecordExpense
        //
        //  Routes expense recording through FinancialManagementService for proper:
        //    - Fiscal period validation
        //    - Category validation
        //    - Journal entry type assignment
        //    - Structured audit logging
        //
        // ────────────────────────────────────────────────────────────────────────

        public class RecordExpenseRequest
        {
            public int FinancialCategoryId { get; set; }
            public DateTime TransactionDate { get; set; }
            public decimal Amount { get; set; }
            public decimal TaxAmount { get; set; }
            public string Description { get; set; }
            public int CurrencyId { get; set; }
            public int? FinancialOfficeId { get; set; }
            public int? ScheduledEventId { get; set; }
            public int? ContactId { get; set; }
            public int? ClientId { get; set; }
            public string ReferenceNumber { get; set; }
            public string Notes { get; set; }
        }


        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/FinancialTransactions/RecordExpense")]
        public async Task<IActionResult> RecordExpenseAsync(
            [FromBody] RecordExpenseRequest request,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "Attempt to record expense by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            // Resolve FinancialManagementService from DI
            var financialService = HttpContext.RequestServices
                .GetService(typeof(Foundation.Scheduler.Services.FinancialManagementService))
                as Foundation.Scheduler.Services.FinancialManagementService;

            if (financialService == null)
            {
                return Problem("Financial management service is not available.");
            }

            var result = await financialService.RecordExpenseAsync(
                userTenantGuid,
                request.FinancialCategoryId,
                request.TransactionDate,
                request.Amount,
                request.TaxAmount,
                request.Description,
                request.CurrencyId,
                request.FinancialOfficeId,
                request.ScheduledEventId,
                request.ContactId,
                request.ClientId,
                request.ReferenceNumber,
                request.Notes,
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            await CreateAuditEventAsync(AuditType.Miscellaneous,
                $"Expense of {request.Amount:C} recorded in category {request.FinancialCategoryId} via FinancialManagementService.");

            return Ok(result.Data);
        }


        // ────────────────────────────────────────────────────────────────────────
        //  RecordRevenue
        //
        //  Routes direct revenue recording through FinancialManagementService.
        //  For ad-hoc revenue not tied to invoices (walk-in sales, permit fees, etc.).
        //
        // ────────────────────────────────────────────────────────────────────────

        public class RecordRevenueRequest
        {
            public int FinancialCategoryId { get; set; }
            public DateTime TransactionDate { get; set; }
            public decimal Amount { get; set; }
            public decimal TaxAmount { get; set; }
            public string Description { get; set; }
            public int CurrencyId { get; set; }
            public int? FinancialOfficeId { get; set; }
            public int? ScheduledEventId { get; set; }
            public int? ContactId { get; set; }
            public int? ClientId { get; set; }
            public string ReferenceNumber { get; set; }
            public string Notes { get; set; }
        }


        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/FinancialTransactions/RecordRevenue")]
        public async Task<IActionResult> RecordRevenueAsync(
            [FromBody] RecordRevenueRequest request,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "Attempt to record revenue by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            // Resolve FinancialManagementService from DI
            var financialService = HttpContext.RequestServices
                .GetService(typeof(Foundation.Scheduler.Services.FinancialManagementService))
                as Foundation.Scheduler.Services.FinancialManagementService;

            if (financialService == null)
            {
                return Problem("Financial management service is not available.");
            }

            var result = await financialService.RecordDirectRevenueAsync(
                userTenantGuid,
                request.FinancialCategoryId,
                request.TransactionDate,
                request.Amount,
                request.TaxAmount,
                request.Description,
                request.CurrencyId,
                request.FinancialOfficeId,
                request.ScheduledEventId,
                request.ContactId,
                request.ClientId,
                request.ReferenceNumber,
                request.Notes,
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            await CreateAuditEventAsync(AuditType.Miscellaneous,
                $"Revenue of {request.Amount:C} recorded in category {request.FinancialCategoryId} via FinancialManagementService.");

            return Ok(result.Data);
        }


        // ────────────────────────────────────────────────────────────────────────
        //  Audit Log
        //
        //  GET /api/FinancialTransactions/AuditLog
        //  Returns structured audit entries with field-level diffs.
        //
        // ────────────────────────────────────────────────────────────────────────

        [HttpGet]
        [Route("api/FinancialTransactions/AuditLog")]
        public async Task<IActionResult> GetAuditLog(
            [FromQuery] int? transactionId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int maxResults = 200,
            CancellationToken cancellationToken = default)
        {
            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return Problem("Your user account is not configured with a tenant.");
            }

            var financialService = HttpContext.RequestServices
                .GetService(typeof(Foundation.Scheduler.Services.FinancialManagementService))
                as Foundation.Scheduler.Services.FinancialManagementService;

            if (financialService == null)
            {
                return Problem("Financial management service is not available.");
            }

            var result = await financialService.GetAuditLogAsync(
                userTenantGuid,
                transactionId,
                fromDate,
                toDate,
                maxResults,
                cancellationToken);

            return Ok(result);
        }


        // ────────────────────────────────────────────────────────────────────────
        //  GL Trial Balance
        //
        //  GET /api/FinancialTransactions/GLTrialBalance
        //  Returns debits/credits per account from the General Ledger.
        //
        // ────────────────────────────────────────────────────────────────────────

        [HttpGet]
        [Route("api/FinancialTransactions/GLTrialBalance")]
        public async Task<IActionResult> GetGLTrialBalance(
            [FromQuery] int? fiscalPeriodId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return Problem("Your user account is not configured with a tenant.");
            }

            var financialService = HttpContext.RequestServices
                .GetService(typeof(Foundation.Scheduler.Services.FinancialManagementService))
                as Foundation.Scheduler.Services.FinancialManagementService;

            if (financialService == null)
            {
                return Problem("Financial management service is not available.");
            }

            var result = await financialService.GetTrialBalanceFromGLAsync(
                userTenantGuid,
                fiscalPeriodId,
                fromDate,
                toDate,
                cancellationToken);

            return Ok(result);
        }

        // ────────────────────────────────────────────────────────────────────────
        //  Service-Routed Update
        //
        //  PUT /api/FinancialTransactions/{id}/Update
        //  Routes transaction edits through FinancialManagementService
        //  for fiscal period validation, category checks, and audit trail.
        //
        // ────────────────────────────────────────────────────────────────────────

        public class UpdateTransactionRequest
        {
            public int FinancialCategoryId { get; set; }
            public DateTime TransactionDate { get; set; }
            public decimal Amount { get; set; }
            public decimal TaxAmount { get; set; }
            public string Description { get; set; }
            public int CurrencyId { get; set; }
            public int? FinancialOfficeId { get; set; }
            public int? ScheduledEventId { get; set; }
            public int? ContactId { get; set; }
            public int? ClientId { get; set; }
            public string ReferenceNumber { get; set; }
            public string Notes { get; set; }
        }


        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/FinancialTransactions/{id}/Update")]
        public async Task<IActionResult> UpdateTransaction(
            int id,
            [FromBody] UpdateTransactionRequest request,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;
            int userId;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
                userId = securityUser.id;
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "Attempt to update transaction by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            var financialService = HttpContext.RequestServices
                .GetService(typeof(Foundation.Scheduler.Services.FinancialManagementService))
                as Foundation.Scheduler.Services.FinancialManagementService;

            if (financialService == null)
            {
                return Problem("Financial management service is not available.");
            }

            var result = await financialService.UpdateTransactionAsync(
                userTenantGuid,
                id,
                userId,
                request.FinancialCategoryId,
                request.TransactionDate,
                request.Amount,
                request.TaxAmount,
                request.Description,
                request.CurrencyId,
                request.FinancialOfficeId,
                request.ScheduledEventId,
                request.ContactId,
                request.ClientId,
                request.ReferenceNumber,
                request.Notes,
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            await CreateAuditEventAsync(AuditType.Miscellaneous,
                $"Transaction {id} updated via FinancialManagementService.");

            return Ok(result.Data);
        }


        // ────────────────────────────────────────────────────────────────────────
        //  Service-Routed Void
        //
        //  POST /api/FinancialTransactions/{id}/Void
        //  Replaces soft-delete with auditable void-with-reason.
        //
        // ────────────────────────────────────────────────────────────────────────

        public class VoidTransactionRequest
        {
            public string Reason { get; set; }
        }


        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/FinancialTransactions/{id}/Void")]
        public async Task<IActionResult> VoidTransaction(
            int id,
            [FromBody] VoidTransactionRequest request,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;
            int userId;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
                userId = securityUser.id;
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "Attempt to void transaction by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            var financialService = HttpContext.RequestServices
                .GetService(typeof(Foundation.Scheduler.Services.FinancialManagementService))
                as Foundation.Scheduler.Services.FinancialManagementService;

            if (financialService == null)
            {
                return Problem("Financial management service is not available.");
            }

            var result = await financialService.VoidTransactionAsync(
                userTenantGuid,
                id,
                userId,
                request?.Reason,
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            await CreateAuditEventAsync(AuditType.Miscellaneous,
                $"Transaction {id} voided via FinancialManagementService. Reason: {request?.Reason}");

            return Ok(result.Data);
        }
    }
}
