using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Foundation.Scheduler.DatabaseUtility
{

    //
    // This is to enable write ahead logging in SQLite to help with concurrency so that readers do not block writers, and to enable faster writes.
    //
    public class SqliteWALModeInterceptor : DbConnectionInterceptor
    {
        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            if (connection is SqliteConnection sqliteConnection)
            {
                using (var command = sqliteConnection.CreateCommand())
                {
                    command.CommandText = "PRAGMA journal_mode=WAL;";
                    command.ExecuteNonQuery();
                }
            }
        }
    }

}