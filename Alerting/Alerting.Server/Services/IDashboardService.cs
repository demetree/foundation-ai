//
// Dashboard Service Interface
//
// Defines the contract for aggregating dashboard data.
//
using System.Threading.Tasks;
using Alerting.Server.Models;

namespace Alerting.Server.Services
{
    /// <summary>
    /// Aggregates data for the Alerting Command Center dashboard.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Gets a complete dashboard summary with all metrics and data.
        /// </summary>
        Task<DashboardSummaryDto> GetSummaryAsync();
    }
}
