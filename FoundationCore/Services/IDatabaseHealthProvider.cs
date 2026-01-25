//
// Database Health Provider Interface
//
// Defines a contract for providing database connection health information.
// Applications can register multiple implementations to monitor all their database contexts.
//
// AI-assisted implementation
//

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Foundation.Services
{
    /// <summary>
    /// 
    /// Interface for providing database health status information.
    /// Implement this for each DbContext that should be monitored in the System Health dashboard.
    /// 
    /// </summary>
    public interface IDatabaseHealthProvider
    {
        /// <summary>
        /// Display name for this database connection (e.g., "Security", "Auditor", "Scheduler")
        /// </summary>
        string Name { get; }


        /// <summary>
        /// Gets the current health status of the database connection
        /// </summary>
        Task<DatabaseHealthInfo> GetHealthAsync();


        /// <summary>
        /// Gets detailed table statistics for this database.
        /// This is an expensive operation and should only be called on-demand.
        /// </summary>
        Task<TableStatisticsInfo> GetTableStatisticsAsync();
    }


    /// <summary>
    /// 
    /// Contains health information about a database connection
    /// 
    /// </summary>
    public class DatabaseHealthInfo
    {
        /// <summary>
        /// Display name for this database (e.g., "Security", "Auditor")
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Status description (e.g., "Connected", "Error", "Not Configured")
        /// </summary>
        public string Status { get; set; }


        /// <summary>
        /// Whether the database connection was successful
        /// </summary>
        public bool IsConnected { get; set; }


        /// <summary>
        /// Database provider name (e.g., "SQL Server", "PostgreSQL", "SQLite")
        /// </summary>
        public string Provider { get; set; }


        /// <summary>
        /// Server or host address (sensitive info like credentials should be stripped)
        /// </summary>
        public string Server { get; set; }


        /// <summary>
        /// Database name if available
        /// </summary>
        public string Database { get; set; }


        /// <summary>
        /// Error message if connection failed
        /// </summary>
        public string ErrorMessage { get; set; }
    }


    /// <summary>
    /// 
    /// Contains table-level statistics for a database
    /// 
    /// </summary>
    public class TableStatisticsInfo
    {
        /// <summary>
        /// Database name this statistics are for
        /// </summary>
        public string DatabaseName { get; set; }


        /// <summary>
        /// Provider name (e.g., "SQL Server")
        /// </summary>
        public string Provider { get; set; }


        /// <summary>
        /// List of table statistics
        /// </summary>
        public List<TableInfo> Tables { get; set; } = new List<TableInfo>();


        /// <summary>
        /// Total number of tables
        /// </summary>
        public int TotalTables { get; set; }


        /// <summary>
        /// Total row count across all tables
        /// </summary>
        public long TotalRows { get; set; }


        /// <summary>
        /// Total size in MB across all tables (0 if not supported by provider)
        /// </summary>
        public decimal TotalSizeMB { get; set; }


        /// <summary>
        /// Whether size information is available for this provider
        /// </summary>
        public bool SizeAvailable { get; set; }


        /// <summary>
        /// Error message if statistics retrieval failed
        /// </summary>
        public string ErrorMessage { get; set; }
    }


    /// <summary>
    /// 
    /// Statistics for a single database table
    /// 
    /// </summary>
    public class TableInfo
    {
        /// <summary>
        /// Table name
        /// </summary>
        public string TableName { get; set; }


        /// <summary>
        /// Approximate row count
        /// </summary>
        public long RowCount { get; set; }


        /// <summary>
        /// Table size in MB (0 if not available)
        /// </summary>
        public decimal SizeMB { get; set; }
    }
}
