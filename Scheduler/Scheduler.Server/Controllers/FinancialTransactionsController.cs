using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.Controllers;
using static Foundation.Auditor.AuditEngine;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// Custom partial class extension of the auto-generated FinancialTransactionsController.
    /// Adds a server-side aggregation endpoint for the financial dashboard summary.
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
    }
}
